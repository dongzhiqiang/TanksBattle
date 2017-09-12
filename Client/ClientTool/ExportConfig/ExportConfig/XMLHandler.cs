public abstract class XMLHandler
{
    public XMLHandler()
    {

    }

    ~XMLHandler()
    {

    }

    public abstract void elementStart(string element, XMLAttributes attributes);
    public abstract void elementEnd(string element);

    public virtual void text(string text)
    {

    }
};

public class XMLHandlerReg : XMLHandler
{
    public delegate void OnElementStart(string element, XMLAttributes attributes);
    public delegate void OnElementEnd(string element);

    System.Collections.Generic.Dictionary<string, OnElementStart> ElementStartList = new System.Collections.Generic.Dictionary<string, OnElementStart>();
    System.Collections.Generic.Dictionary<string, OnElementEnd> ElementEndList = new System.Collections.Generic.Dictionary<string, OnElementEnd>();

    public void RegElementStart(string element, OnElementStart start)
    {
        ElementStartList.Add(element, start);
    }

    public void RegElementEnd(string element, OnElementEnd end)
    {
        ElementEndList.Add(element, end);
    }

    public override void elementStart(string element, XMLAttributes attributes)
    {
        OnElementStart start = null;
        if (ElementStartList.TryGetValue(element, out start))
        {
            start(element, attributes);
        }
    }

    public override void elementEnd(string element)
    {
        OnElementEnd end = null;
        if (ElementEndList.TryGetValue(element, out end))
        {
            end(element);
        }
    }
}