public static class AvatarHelper
{
    public static ImageSource Get(string? profilePicture)
    {
        if (string.IsNullOrWhiteSpace(profilePicture))
            return "avatar1.png";

        if (profilePicture.StartsWith("avatar", StringComparison.OrdinalIgnoreCase))
            return ImageSource.FromFile(profilePicture);

        if (Uri.TryCreate(profilePicture, UriKind.Absolute, out var uri))
            return ImageSource.FromUri(uri);

        return "avatar1.png";
    }
}
