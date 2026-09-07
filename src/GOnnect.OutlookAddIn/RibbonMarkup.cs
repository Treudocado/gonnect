namespace GOnnect.OutlookAddIn
{
    internal static class RibbonMarkup
    {
        public const string Explorer = @"<?xml version=""1.0"" encoding=""utf-8""?>
<customUI xmlns=""http://schemas.microsoft.com/office/2009/07/customui"">
  <contextMenus>
    <contextMenu idMso=""ContextMenuContactItem"">
      <dynamicMenu id=""GOnnectDialContactMenu""
                   label=""Mit GOnnect anrufen""
                   imageMso=""Call""
                   insertAfterMso=""DialMenu""
                   getVisible=""GetMenuVisible""
                   getContent=""GetPhoneMenuContent""
                   invalidateContentOnDrop=""true"" />
    </contextMenu>
    <contextMenu idMso=""ContextMenuFlaggedContactItem"">
      <dynamicMenu id=""GOnnectDialFlaggedContactMenu""
                   label=""Mit GOnnect anrufen""
                   imageMso=""Call""
                   insertAfterMso=""DialMenu""
                   getVisible=""GetMenuVisible""
                   getContent=""GetPhoneMenuContent""
                   invalidateContentOnDrop=""true"" />
    </contextMenu>
  </contextMenus>
</customUI>";
    }
}

