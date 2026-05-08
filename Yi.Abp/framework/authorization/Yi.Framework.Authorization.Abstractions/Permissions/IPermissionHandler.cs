namespace Yi.Framework.Authorization.Abstractions.Permissions
{
    public interface IPermissionHandler
    {
        Task<bool> IsGrantedAsync(string permission);
    }
}
