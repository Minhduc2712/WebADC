namespace ErpOnlineOrder.Application.PrintTemplates
{

    public interface IPrintTemplate<TData>
    {
        string Name { get; }

        string DisplayName { get; }

        string DocumentType { get; }

        byte[] ToPdf(TData data, PrintTemplateSettings settings);
        byte[] ToDocx(TData data, PrintTemplateSettings settings);
        byte[] ToXml(TData data, PrintTemplateSettings settings);
    }
}
