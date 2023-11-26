namespace sfa.sale.generator.web.WebHelpers;

/// <summary>
/// Define javascript functions here.
/// If further hierarchy is needed for a library with various functions, nest a new class.
/// </summary>
public static class JSFunctions
{
    public static class Utils
    {
        public static readonly string NAMESPACE = "window.utils";
        public static readonly string CLIPBOARD_COPY = $"{NAMESPACE}.clipboard.copyText";
    }
}