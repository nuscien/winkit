using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Security;
using Trivial.Text;
using Trivial.UI;
using Trivial.Web;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Trivial.Demo;

/// <summary>
/// The homepage
/// </summary>
public sealed partial class HomePage : Page
{
    private readonly LocalWebAppHost webAppHost;

    /// <summary>
    /// Initializes a new instance of the HomePage class.
    /// </summary>
    public HomePage()
    {
        InitializeComponent();
        for (var i = 0; i < 1000; i++)
        {
            TextViewElement.Append(new[]
            {
                "Welcome!",
                "This is a demo for Trivial.WindowsKit library.",
                "It can be used for pages of news, video and product.",
                string.Empty,
                "This demo shows basic visual samples based on some 3rd-party data sources.",
                "Copyrights of these data sources are reserved by their owner.",
                string.Empty
            });
        }

        FileBrowserElement.NavigateAsync(new DirectoryInfo("C:\\"));
    }

    private async Task<LocalWebAppHost> CreateWebAppHostAsync(LocalWebAppVerificationOptions verifyOptions = LocalWebAppVerificationOptions.SkipException)
        => webAppHost ?? webAppHost = await LocalWebAppHost.LoadAsync(
            new DirectoryInfo("LocalWebApp"),
            new LocalWebAppOptions("filebrowser", "filebrowser", RSASignatureProvider.CreateRS256(@"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAz5GmwlD/BMX2Pix4Vkv9
F8LGzQaTjMsRZqvgWbPaoeEIKez9GKM56FPwH4aTZOO/ibkaRQozxdzloRQmSlJJ
xvqMVqm+upzY7z1Uu3UDsyVK859m5St8CE0VUX7sbd9Ywe3hoC+AHgewx7XIgxm8
o3QcDnRHUPX8YbkZoqyq0vzFmmJ6Z5D39ykx2VZdHRHWp1CLb6lYOm2AA8fU9PTK
zjl57kX+Ex4px9Fy199+sD/0sA2zag+RoWeorz+nYbInW49MU/Z/JXLeLJ5fX7f3
vpqoRRlGJLj5FwMF5OzElKJkPmqmpaMp3Eo9QNrL8bhKDcSGggV/PCv7L8+QdZ7z
yQIDAQAB
-----END PUBLIC KEY-----"), new()
            {
                Url = "http://localhost/test/LocalWebApp.json"
            }),
            verifyOptions);

    private void LaunchWebAppClick(object sender, RoutedEventArgs e)
    {
        var window = new LocalWebAppWindow()
        {
            Title = "Loading…",
            IsDevEnvironmentEnabled = true
        };
        var hostTask = CreateWebAppHostAsync();
        _ = window.LoadAsync(hostTask, host => _ = host?.UpdateAsync());
        var appWin = VisualUtility.TryGetAppWindow(window);
        window.Activate();
    }

    private void SignWebAppClick(object sender, RoutedEventArgs e)
        => _ = SignWebAppClickAsync();

    private async Task SignWebAppClickAsync()
    {
        var key = @"-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDPkabCUP8ExfY+
LHhWS/0XwsbNBpOMyxFmq+BZs9qh4Qgp7P0YoznoU/AfhpNk47+JuRpFCjPF3OWh
FCZKUknG+oxWqb66nNjvPVS7dQOzJUrzn2blK3wITRVRfuxt31jB7eGgL4AeB7DH
tciDGbyjdBwOdEdQ9fxhuRmirKrS/MWaYnpnkPf3KTHZVl0dEdanUItvqVg6bYAD
x9T09MrOOXnuRf4THinH0XLX336wP/SwDbNqD5GhZ6ivP6dhsidbj0xT9n8lct4s
nl9ft/e+mqhFGUYkuPkXAwXk7MSUomQ+aqaloyncSj1A2svxuEoNxIaCBX88K/sv
z5B1nvPJAgMBAAECggEAUu1G6WVArWCFo4tSvG95ey+3CxxwgJR0rEdIx63CUGA+
SbnD2D8GGJrIWWADrRAMavKH20NbMdax6yvIrHK5xQQ+YzVH3Phi9xnSq13xj3X7
vt9VVYOM9ygMt1V1EeRkan4mYT/4+IZsCy3GIRJ8OfVebCvqfh74qPYxlrtTOB1K
edhb5ToERpkMK1bDJEILfFCGyVJzJXQkyKOK4uUQM3jBxWWqE9JEoCSBvVW5+xVY
yHrqisuUz7EIhttQFj+1wPhNKQVMv7rr6/vKxygJ33X9xJWPVENzRi5ThqsYnoXi
Yg35kuLJH5ivfSDiAEhAP9AMzRd2B25RlebR8FBRDQKBgQDg9XLoXZhF2piTChmb
4fnglF5VDZqT4kl0fKEIO6ZbgWKbWb8s7MzvMaTNhJnF8sQ6XUU1NB9xyK7Ahtcp
NRoYjbH8DwEAHhchZyAJVPJ0dlWizvey9hKZzmvNneDktifsn+xzqvRIjADjVEuj
vmIrFqAn2QiFpc9wH4SxDS+hnwKBgQDsNepwnUIUW6eodLqsJBNv0+HYHIdbJqAi
/GNvAPmJhwtSjrRxdEJD4X9pf9K3r6KpmJYmN61ZWqyk2gtdQhiou12AwIU8Lkj/
ZSkC+W2nRI441fIi2iWC7PEdk185NVNW2xljfuiG96CkemK3v/EbKVEB0CnVCmzD
fwvR+PsBlwKBgQC4pL5MO4Zgz6usBP5AFJsk2qMS7LeT6oigNCt4tn01Xl2xZVil
ZzhOnFDI363X7AtkXGoR4VZt7mqBXCv+hreEr8kHOsl3bztND3gcML1RGk/v8jEd
kxxxYhzaCFwvXdQnRJyv1AHuCfwwm1/6Zqns9AVAr8Nu70n0neor6MbPwQKBgBXz
JKf2VQ+jPL8wqbAZYh0AKXp1nDZiLntRzMOh6Y5YGDtBu47XaNj5+WcKU8Bx98Ge
xkUi417sSCLBiFDQNY5oatXuDfN7sZjaA6edGg1zF2w8pVWLw/SYpAdFjJG6XNYz
YfaW8nCoTis6nDXLBlKp0jdC6sA7ScQY6DZI1rpdAoGBALiS3TRy7rvQlFzvZPB/
QRAWXTfXo8qknqUuTfe0/qasbEvxxQJsJmejHDFP1LrbsHMuE64AFtqnvy/3nO4r
WlFiraboISACv3BoFD2WFVi5u9PKShkyaMnuOdQWwg1aeD0qCd0HF47omHnqrzWB
CuJHsDqtVkOkzyPImHfrR6zU
-----END PRIVATE KEY-----";
        var host = await CreateWebAppHostAsync(LocalWebAppVerificationOptions.Disabled);
        host.Sign(RSASignatureProvider.CreateRS256(key), out var hasWritenFile);
        if (!hasWritenFile)
        {
            return;
        }

        host.Package();
    }
}
