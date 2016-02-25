## What is this?

CorrelatorSharp.Mvc5 is an add-on for ASP.NET MVC 5 that enables support for [CorrelatorSharp](http://correlatorsharp.github.io). 

## Get it


|   Where    |    What   |
|-------------|-------------|
| NuGet       | [CorrelatorSharp.Mvc5](https://www.nuget.org/packages/CorrelatorSharp.Mvc5/)
| Latest Build (master)      |   [![Build status](https://ci.appveyor.com/api/projects/status/akympc06qgq33n3k/branch/master?svg=true)](https://ci.appveyor.com/project/CorrelatorSharp/correlatorsharp-mvc5/branch/master)  |


## Using it

**Sample:** https://github.com/CorrelatorSharp/CorrelatorSharp.Mvc5/tree/master/CorrelatorSharp.Mvc5.Sample


### Step 1: Add the Correlation Filter


```csharp
public class FilterConfig
{
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
        filters.Add(new CorrelationIdActionFilter());
    }
}
```

### Step 2: Consume Correlation

The filter will automatically initialize the current activity scope for correlation based on either a `X-Correlation-Id` http header in the request or a new random activity id. They will also inject a `X-Correlation-Id` header into the response.

```csharp
public ActionResult Test()
{
    return Json(ActivityScope.Current.Id);
}
```

A very simple example of passing in a correlation id as part of a jQuery AJAX call:

```html
<script>
    $(function() {
        $("#sendRequest").on("click", function () {
            $.ajax({
				beforeSend: function (xhr) {
                    xhr.setRequestHeader('X-Correlation-Id', '1234');
                },
                url: encodeURI('/home/test'),
                type: 'POST',
                data: {},
                contentType: 'application/x-www-form-urlencoded',
                success: function (data) { alert("server responded with correlation id: " + data); },
                error: function () { },
            });
        });
    });
</script>

<div>
    <button id="sendRequest">Send Request with Correlation Id "1234"</button>
</div>
```

