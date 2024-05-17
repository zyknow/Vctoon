using Microsoft.Extensions.Localization;
using Vctoon.Localization.Vctoon;
using Volo.Abp.DependencyInjection;

namespace Vctoon.Blazor.Client.Services;

public class DateTimeFormatService(IStringLocalizer<VctoonResource> L) : ITransientDependency
{
    public string FormatCreateDate(DateTime date)
    {
        DateTime now = DateTime.Now;
        
        TimeSpan diff = now - date;
        if (diff.TotalMinutes < 1)
        {
            return L["DateFormat:Moment"];
        }
        
        if (diff.TotalHours < 1)
        {
            return L["CreateDateFormat", @$"{(int)diff.TotalMinutes} {L["DateFormat:BeforeMinute"]}"];
        }
        
        if (diff.TotalDays < 1)
        {
            return L["CreateDateFormat", @$"{(int)diff.TotalHours} {L["DateFormat:BeforeHour"]}"];
        }
        
        if (diff.TotalDays < 30)
        {
            return L["CreateDateFormat", @$"{(int)diff.TotalDays} {L["DateFormat:BeforeDay"]}"];
        }
        
        if (diff.TotalDays < 365)
        {
            return L["CreateDateFormat", @$"{(int)diff.TotalDays / 30} {L["DateFormat:BeforeMonth"]}"];
        }
        
        return L["CreateDateFormat", @$"{(int)diff.TotalDays / 365} {L["DateFormat:BeforeYear"]}"];
    }
    
    public string FormatModifyDate(DateTime date)
    {
        DateTime now = DateTime.Now;
        
        TimeSpan diff = now - date;
        if (diff.TotalMinutes < 1)
        {
            return L["DateFormat:Moment"];
        }
        
        if (diff.TotalHours < 1)
        {
            return L["ModifyDateFormat", @$"{(int)diff.TotalMinutes} {L["DateFormat:BeforeMinute"]}"];
        }
        
        if (diff.TotalDays < 1)
        {
            return L["ModifyDateFormat", @$"{(int)diff.TotalHours} {L["DateFormat:BeforeHour"]}"];
        }
        
        if (diff.TotalDays < 30)
        {
            return L["ModifyDateFormat", @$"{(int)diff.TotalDays} {L["DateFormat:BeforeDay"]}"];
        }
        
        if (diff.TotalDays < 365)
        {
            return L["ModifyDateFormat", @$"{(int)diff.TotalDays / 30} {L["DateFormat:BeforeMonth"]}"];
        }
        
        return L["ModifyDateFormat", @$"{(int)diff.TotalDays / 365} {L["DateFormat:BeforeYear"]}"];
    }
}