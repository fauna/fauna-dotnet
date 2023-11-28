namespace Fauna.Types;

public class Module
{
    public string Name { get; private set; }

    public Module(string name)
    {
        Name = name;
    }
}
