using System;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using MB;

namespace Avalonia.Miniblink;

public class MiniblinkBrowser : NativeControlHost
{
    private WebView? _webView;

    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly StyledProperty<string?>
        SourceProperty = AvaloniaProperty.Register<MiniblinkBrowser, string?>(string.Empty);
    
    public EventHandler<LoadingFinishEventArgs>? LoadingFinshed { get; set; }
    public EventHandler<UrlChangeEventArgs>? UrlChanged { get; set; }
    public EventHandler<NavigateEventArgs>? Navigated { get; set; }
    public EventHandler<DocumentReadyEventArgs>? DocumentReady { get; set; }

    static MiniblinkBrowser()
    {
        SourceProperty.Changed.AddClassHandler<MiniblinkBrowser>((s, e) => s.LoadUrl(e.NewValue as string));
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var hwnd = base.CreateNativeControlCore(parent);
        _webView?.Dispose();
        _webView = new WebView();
        _webView.Bind(hwnd.Handle);

        _webView.OnLoadingFinish += HandleLoadingFinished;
        _webView.OnURLChange += HandleUrlChanged;
        _webView.OnNavigate += HandleNavigated;
        _webView.OnDocumentReady += HandleDocumentReady;
        _webView.NavigationToNewWindowEnable = false;

        LoadUrl(Source);

        return hwnd;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_webView is not null)
        {
            _webView.OnLoadingFinish -= HandleLoadingFinished;
            _webView.OnURLChange -= HandleUrlChanged;
            _webView.OnNavigate -= HandleNavigated;
            _webView.OnDocumentReady -= HandleDocumentReady;
        }
        _webView?.Dispose();
        _webView = null;

        base.DestroyNativeControlCore(control);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        _webView?.SetDeviceParameter("screen.width", string.Empty, (int)e.NewSize.Width);
        _webView?.SetDeviceParameter("screen.height", string.Empty, (int)e.NewSize.Height);
        base.OnSizeChanged(e);
    }

    public void LoadUrl(string? url) => Dispatcher.UIThread.Invoke(() =>
    {
        SetValue(SourceProperty, url);
        if (string.IsNullOrEmpty(url)) return;
        _webView?.LoadURL(url);
    });

    public void SetZoom(float scale)
    {
        if (_webView is null) return;
        _webView.ZoomFactor = scale;
    }

    public void InvokeJavascript(string js)
    {
        if (_webView is null) return;
        _webView.RunJS(js);
    }
    
    #region Callbacks
    private void HandleUrlChanged(object? sender, UrlChangeEventArgs e) => UrlChanged?.Invoke(this,e);
    private void HandleLoadingFinished(object? sender, LoadingFinishEventArgs e) => LoadingFinshed?.Invoke(this,e);
    private void HandleNavigated(object sender, NavigateEventArgs e) => Navigated?.Invoke(this, e);
    private void HandleDocumentReady(object sender, DocumentReadyEventArgs e) => DocumentReady?.Invoke(this, e);

    #endregion
}