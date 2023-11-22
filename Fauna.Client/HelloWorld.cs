namespace Fauna.Client;

public class HelloWorld
{
  public string? MyField { get; set; }

  public HelloWorld(string? myField = null)
  {
    this.MyField = myField;
  }
}
