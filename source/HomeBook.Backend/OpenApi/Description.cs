namespace HomeBook.Backend.OpenApi;

public class Description(params string[] descriptionLines)
{
    public override string ToString()
    {
        if (descriptionLines.Length == 0)
            return string.Empty;
        return string.Join(Environment.NewLine + Environment.NewLine, descriptionLines);
    }

    public static implicit operator string(Description d)
        => d.ToString();
}
