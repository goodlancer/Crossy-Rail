using System;

[AttributeUsage(AttributeTargets.Class)]
public class RequiredWordAttribute : Attribute
{
    public string[] requiredWords;

    public RequiredWordAttribute(params string[] requiredWords)
    {
        this.requiredWords = requiredWords;
    }
}
