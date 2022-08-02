[Local Web App](../localwebapp)

# Customized native extends

Native app developers can implement the command handler for customized extensions. It is based on message communication between host app and webpage. JS can get the command handler by `localWebApp.getCommandHandler` function with its identifier and it returns a proxy of the native implementation to send request and get response.

In native-side, developers need implement interface `Trivial.Web.ILocalWebAppCommandHandler` and register the instance into the page or window. The interface contains a method `Process` to process the request message as `LocalWebAppRequestMessage` and need return result in `LocalWebAppResponseMessage`. The method also pass its manifest for reference. The request from JS will be handled by it and the result wil be sent back to JS.
