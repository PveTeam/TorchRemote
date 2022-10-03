using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData.Binding;
using Json.Schema;
using ReactiveUI;
using TorchRemote.Models.Responses;
using TorchRemote.Models.Shared.Settings;
using TorchRemote.Services;
using Api = TorchRemote.Models.Shared.Settings;

namespace TorchRemote.ViewModels.Server;

public class SettingViewModel : ViewModelBase
{
    private readonly ApiClientService _service;
    public ObjectProperty Setting { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;

    public SettingViewModel(ApiClientService service)
    {
        _service = service;
        
        /*this.WhenValueChanged(x => x.FullName, false)
            .Select(b => Observable.FromAsync(() => _service.Api.GetSetting(b!)))
            .Concat()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Where(b => b.Content is not null)
            .Select(b =>
            {
                var value = b.Content!;
                
                
            })*/
    }
}

public interface IProperty<in TProperty> where TProperty : PropertyBase
{
    public static abstract Property FromProperty(TProperty property);
}

public abstract class Property : ViewModelBase
{
    public abstract string TypeName { get; }
}

public class IntegerProperty : Property, IProperty<Api.IntegerProperty>
{
    public int Value { get; set; }
    public override string TypeName => "integer";
    
    public static Property FromProperty(Api.IntegerProperty property)
    {
        return new IntegerProperty
        {
            Value = property.Value.GetValueOrDefault()
        };
    }
}

public class StringProperty : Property, IProperty<Api.StringProperty>
{
    public string Value { get; set; } = "value";
    public override string TypeName => "string";
    public static Property FromProperty(Api.StringProperty property)
    {
        return new StringProperty
        {
            Value = property.Value ?? string.Empty
        };
    }
}

public class BooleanProperty : Property, IProperty<Api.BooleanProperty>
{
    public bool Value { get; set; }
    public override string TypeName => "boolean";
    public static Property FromProperty(Api.BooleanProperty property)
    {
        return new BooleanProperty
        {
            Value = property.Value.GetValueOrDefault()
        };
    }
}

public class NumberProperty : Property, IProperty<Api.NumberProperty>
{
    public double Value { get; set; }
    public override string TypeName => "number";
    public static Property FromProperty(Api.NumberProperty property)
    {
        return new NumberProperty
        {
            Value = property.Value.GetValueOrDefault()
        };
    }
}

public class DateTimeProperty : Property, IProperty<Api.DateTimeProperty>
{
    public DateTime Value { get; set; }
    public override string TypeName => "date-time";
    public static Property FromProperty(Api.DateTimeProperty property)
    {
        return new DateTimeProperty
        {
            Value = property.Value ?? DateTime.Now
        };
    }
}

public class DurationProperty : Property, IProperty<Api.DurationProperty>
{
    public TimeSpan Value { get; set; }
    public override string TypeName => "duration";
    public static Property FromProperty(Api.DurationProperty property)
    {
        return new DurationProperty
        {
            Value = property.Value ?? TimeSpan.Zero
        };
    }
}

public class UuidProperty : Property, IProperty<Api.UuidProperty>
{
    public Guid Value { get; set; }
    public override string TypeName => "uuid";
    public static Property FromProperty(Api.UuidProperty property)
    {
        return new UuidProperty
        {
            Value = property.Value ?? Guid.Empty
        };
    }
}

public class UriProperty : Property, IProperty<Api.UriProperty>
{
    public string Value { get; set; } = null!;
    public override string TypeName => "uri";
    public static Property FromProperty(Api.UriProperty property)
    {
        return new UriProperty
        {
            Value = property.Value?.ToString() ?? string.Empty
        };
    }
}

public class EnumProperty : Property, IProperty<Api.EnumProperty>
{
    public string Value { get; set; } = "value";
    public IEnumerable<(string Name, string Value)> EnumValues { get; set; } = ArraySegment<(string Name, string Value)>.Empty;
    public override string TypeName => "enum";
    public static Property FromProperty(Api.EnumProperty property)
    {
        return new EnumProperty
        {
            Value = property.Value
        };
    }
}

public class ObjectProperty : Property, IProperty<Api.ObjectProperty>
{
    public ObservableCollection<Property> Properties { get; init; } = new();
    public override string TypeName => "object";
    public static Property FromProperty(Api.ObjectProperty property)
    {
        return new ObjectProperty();
    }
}