[!(Trivial.WindowsKit)](./assets/logo.png)

# Trivial.WindowsKit

[![NuGet package](https://img.shields.io/nuget/dt/Trivial.WindowsKit?label=nuget+downloads)](https://www.nuget.org/packages/Trivial.WindowsKit)
[![GitHub Repository](./assets/badge_GitHub-Repo.svg)](https://github.com/nuscien/winkit)
[![MIT licensed](./assets/badge_lisence_MIT.svg)](https://github.com/nuscien/winkit/blob/master/LICENSE)
![.NET 6 - Windows 10](./assets/badge_NET_6_Win10.svg)

Includes some complex controls and utilities on WinUI 3.
It is useful to create the client with collection pages of news, videos and products.

## Components and utils

Following are the commonly used controls.

- `TileItem` shows image and text.
- `TileCollection` is a horizontal list of tile.
- `CommentView` with poster information and comment.
- `BlockHeader` is a header for a group.
- `TextButton` is a button with different states that you can customize colors.
- `TextView` is a read-only text view for string with a greate number of line.
- `SettingsExpanderHeader` is the expander header like the one in Windows Settings.
- `FileListView` is a lite file browser.
- `TabbedWebView` is a lite web browser powered by Microsoft Edge WebView2.

And following are some utilities.

- `DependencyObjectProxy` is a helper for implementing custom control or user control.
- `VisualUtility` is a helper class for framework element.
- `BaseJsonPagingLoader` is a base class to implement the JSON data loader with paging.
- `JsonWebCacheClient` is a HTTP client with cache management to load JSON data.

## Local web app

Local web app is an app framework to launch HTML+JS locally with native capability and auto-update.
[See more](./localwebapp).
