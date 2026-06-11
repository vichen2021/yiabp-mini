/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers
 * for more information concerning the license and the contributors participating to this project.
 */

namespace Yi.Framework.AspNetCore.Authentication.OAuth.Gitee;

/// <summary>
/// Default values used by the Gitee authentication middleware.
/// </summary>
public static class GiteeAuthenticationDefaults
{
    /// <summary>
    /// Default value for the authentication scheme name.
    /// </summary>
    public const string AuthenticationScheme = "Gitee";

    /// <summary>
    /// Default value for the authentication scheme display name.
    /// </summary>
    public static readonly string DisplayName = "Gitee";

    /// <summary>
    /// Default value for the claims issuer.
    /// </summary>
    public static readonly string Issuer = "Gitee";

    /// <summary>
    /// Default value for the callback path.
    /// </summary>
    public static readonly string CallbackPath = "/signin-gitee";

    /// <summary>
    /// Default value for the authorization endpoint.
    /// </summary>
    public static readonly string AuthorizationEndpoint = "https://gitee.com/oauth/authorize";

    /// <summary>
    /// Default value for the token endpoint.
    /// </summary>
    public static readonly string TokenEndpoint = "https://gitee.com/oauth/token";

    /// <summary>
    /// Default value for the user information endpoint.
    /// </summary>
    public static readonly string UserInformationEndpoint = "https://gitee.com/api/v5/user";

    /// <summary>
    /// Default value for the user emails endpoint.
    /// </summary>
    public static readonly string UserEmailsEndpoint = "https://gitee.com/api/v5/emails";
}
