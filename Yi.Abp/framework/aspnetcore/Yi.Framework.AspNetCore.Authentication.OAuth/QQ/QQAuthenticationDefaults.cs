/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers
 * for more information concerning the license and the contributors participating to this project.
 */

namespace Yi.Framework.AspNetCore.Authentication.OAuth.QQ;

/// <summary>
/// Default values for QQ authentication.
/// </summary>
public static class QQAuthenticationDefaults
{
    /// <summary>
    /// Default value for the authentication scheme name.
    /// </summary>
    public const string AuthenticationScheme = "QQ";

    /// <summary>
    /// Default value for the authentication scheme display name.
    /// </summary>
    public static readonly string DisplayName = "QQ";

    /// <summary>
    /// Default value for the claims issuer.
    /// </summary>
    public static readonly string Issuer = "QQ";

    /// <summary>
    /// Default value for the callback path.
    /// </summary>
    public static readonly string CallbackPath = "/signin-qq";

    /// <summary>
    /// Default value for the authorization endpoint.
    /// </summary>
    public static readonly string AuthorizationEndpoint = "https://graph.qq.com/oauth2.0/authorize";

    /// <summary>
    /// Default value for the token endpoint.
    /// </summary>
    public static readonly string TokenEndpoint = "https://graph.qq.com/oauth2.0/token";

    /// <summary>
    /// Default value for the user identification endpoint.
    /// </summary>
    public static readonly string UserIdentificationEndpoint = "https://graph.qq.com/oauth2.0/me";

    /// <summary>
    /// Default value for the user information endpoint.
    /// </summary>
    public static readonly string UserInformationEndpoint = "https://graph.qq.com/user/get_user_info";
}
