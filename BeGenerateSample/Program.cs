using System.ComponentModel;
using BeGenerateSample;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class Testing
{
    public Testing()
    {
        Test service = new SampleService();
        service.Test();
    }
}
