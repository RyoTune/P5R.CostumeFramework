using CriFs.V2.Hook.Interfaces;

namespace P5R.CostumeFramework;

internal static class ICriFsRedirectorApiExtensions
{
    public static void AddBind(
        this ICriFsRedirectorApi api,
        string file,
        string bindPath,
        string modId)
    {
        api.AddBindCallback(context =>
        {
            context.RelativePathToFileMap[$@"R2\{bindPath}"] = new()
            {
                new()
                {
                    FullPath = file,
                    LastWriteTime = DateTime.UtcNow,
                    ModId = modId,
                },
            };

            Log.Verbose($"Bind: {bindPath}\nFile: {file}");
        });
    }
}
