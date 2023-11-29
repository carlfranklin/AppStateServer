/// <summary>
/// This record is used to pass the name of the property that changed and the new value of the property.
/// </summary>
/// <param name="PropertyName"></param>
/// <param name="NewValue"></param>
public record StatePropertyChangedArgs(string PropertyName, object? NewValue);