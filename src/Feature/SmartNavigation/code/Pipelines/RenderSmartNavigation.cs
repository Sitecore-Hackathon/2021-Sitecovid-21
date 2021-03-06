﻿using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Feature.SmartNavigation.Models;
using Feature.SmartNavigation.Services;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;

namespace Feature.SmartNavigation.Pipelines
{
    public class RenderSmartNavigation
    {
        private readonly ISmartNavigationService smartNavigationService;
        private const string SectionName = "Smart Navigation";
        private const string HeaderIconPath = "/~/icon/office/16x16/lightbulb_on.png";
        private const string IconPath = "/-/icon/Office/16x16/navigate_right.png.aspx";

        public RenderSmartNavigation(ISmartNavigationService smartNavigationService)
        {
            this.smartNavigationService = smartNavigationService;
        }

        private Item CurrentItem { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Is checked by Assert")]
        public void Process(RenderContentEditorArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            var parentControl = args.Parent;
            var editorFormatter = args.EditorFormatter;
            CurrentItem = args.Item;

            if (CurrentItem == null)
            {
                return;
            }

            var suggestedItems = smartNavigationService.GetSuggestions(CurrentItem, 5);

            if (suggestedItems == null || !suggestedItems.NavigationsFromItem.Any() && !suggestedItems.NavigationsFromItem.Any() && suggestedItems.LastItem == null)
            {
                return;
            }

            editorFormatter.RenderSectionBegin(parentControl, SectionName, SectionName, SectionName, HeaderIconPath, false, true);
            RenderNavigation(editorFormatter, parentControl, suggestedItems, CurrentItem);
            editorFormatter.RenderSectionEnd(parentControl, true, false);
        }

        private static void RenderNavigation(EditorFormatter editorFormatter, Control parentControl, SuggestionSet suggestions, Item currentItem)
        {
            var suggestionExists = suggestions.NavigationsFromItem.Any() || suggestions.NavigationsToItem.Any();
            var suggestionsHtml = string.Empty;
            if (suggestionExists)
            {
                var fromItemsOutput = GetItemListHtml(suggestions.NavigationsFromItem);
                var toItemsOutput = GetItemListHtml(suggestions.NavigationsToItem);
                suggestionsHtml = suggestions.NavigationsFromItem.Any() || suggestions.NavigationsToItem.Any() ? $"<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tbody><tr><td style=\"vertical-align: top;\"><div style=\"font-size: 14px; margin-bottom: 10px;\"><b>You might want to work on these next</b></div>{fromItemsOutput}</td><td style=\"vertical-align: top;\"><div style=\"font-size: 14px; margin-bottom: 10px;\"><b>You might want to go back to these</b></div>{toItemsOutput}</td></tr></tbody></table>" : string.Empty;
            }

            var splitterHtml = suggestionExists ? "<hr style=\"margin:10px 0\"/>" : string.Empty;
            var lastItemOutput = suggestions.LastItem != null && suggestions.LastItem.ItemId != currentItem.ID.Guid ? $"{splitterHtml}<div style=\"font-size: 14px; margin-bottom: 10px;\"><b>The last item you have worked on</b> </div><div style=\"padding-left: 7px;\"><a href=\"#\" class=\"scLink\" title=\"{suggestions.LastItem.Path}\" onclick=\"javascript:return scForm.invoke(&quot;item:load(id={{{suggestions.LastItem.ItemId}}},language=en,version=1)&quot;)\"><span><img src=\"/-/icon/Office/16x16/navigate_left.png.aspx\" width=\"16\" height=\"16\" class=\"scContentTreeNodeIcon\" alt=\"\" border=\"0\"><b>{suggestions.LastItem.Name}</b> - [{suggestions.LastItem.Path}]</span></a></div>" : string.Empty;
            var output = $"<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" class=\"scEditorFieldMarker\"><tbody><tr><td id=\"FieldMarkerFIELD2123021\" class=\"scEditorFieldMarkerBarCell\"><img src=\"/sitecore/images/blank.gif\" width=\"4px\" height=\"1px\"></td><td class=\"scEditorFieldMarkerInputCell\">{suggestionsHtml}{lastItemOutput}</td></tr></tbody></table>";
            editorFormatter.AddLiteralControl(parentControl, output);
        }

        private static string GetItemListHtml(IEnumerable<NavigationItem> items) => items.Any() ?  $"<ul style=\"padding-left:30px; list-style-image: url('{IconPath}');\">{string.Join(string.Empty, items.Select(GetItemLink))}</ul>" : "<div>We could not find anything. Let's get to work!</div>";
        private static string GetItemLink(NavigationItem item) =>
            $"<li><a href=\"#\" class=\"scLink\" title=\"{item.Path}\"\r\n    onclick=\"javascript:return scForm.invoke(&quot;item:load(id={{{item.ItemId}}},language=en,version=1)&quot;)\"><span\r\nstyle=\"top:-4px; position:relative;\"><b>{item.Name}</b> -\r\n[{item.Path}]</span></a></li>";
    }
}