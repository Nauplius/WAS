var notifyId = '';

function DelSourceHelp() {
    notifyId = SP.UI.Notify.addNotification("Delete source document on conversion.", false);
}

function PdfHelp(value) {
    var helpText = "";
    switch (value) {
        case "BitmapEmbeddedFonts":
            helpText = "Allow unembeddable fonts to be bitmaped.";
            break;
        case "IncludeDocumentProperties":
            helpText = "Include document properties.";
            break;
        case "IncludeDocumentStructure":
            helpText = "Include document structure.";
            break;
        case "OptimizeForMinimumSize":
            helpText = "Optimize the output for minimim size.";
            break;
        case "UsePdfA":
            helpText = "Use PDF/A, an ISO standard for<br /> long-term document archival.";
            break;
    }
    notifyId = SP.UI.Notify.addNotification(helpText, false);
}

function WordHelp(value) {
    var helpText = "";
    switch (value) {
        case "AddThumbnail":
            helpText = "Saves the document with a thumbnail.";
            break;
        case "EmbedFonts":
            helpText = "Embeds fonts within the document.";
            break;
        case "UpdateFields":
            helpText = "Automatically updates fields <br /> within the document.";
            break;
    }
    notifyId = SP.UI.Notify.addNotification(helpText, false);
}

function BookmarkHelp(ddl) {
    var helpText = "";

    var bookmarkTypeDdl = document.getElementById(ddl);
    var value = bookmarkTypeDdl.options[bookmarkTypeDdl.selectedIndex].value;

    if (notifyId != '') {
        RemoveHelp();
    }

    switch (value) {
        case "None":
            helpText = "Specifies the None option, <br /> which excludes bookmarks in the output.";
            break;
        case "Headings":
            helpText = "Specifies the Headings option, <br /> which converts Word headings into <br />" +
                "bookmarks in the output.";
            break;
        case "Bookmarks":
            helpText = "Specifies the Bookmarks option, <br /> which converts Word bookmarks into <br />" +
                "bookrmarks in the output.";
    }

    notifyId = SP.UI.Notify.addNotification(helpText, false);
}

function BalloonHelp(ddl) {
    var helpText = "";

    var balloonTypeDdl = document.getElementById(ddl);
    var value = balloonTypeDdl.options[balloonTypeDdl.selectedIndex].value;

    if (notifyId != '') {
        RemoveHelp();
    }

    switch (value) {
        case "AlwaysUse":
            helpText = "Specifies that revisions are <br /> always shown in balloons.";
            break;
        case "Inline":
            helpText = "Specifies that all revisions are <br /> shown inline.";
            break;
        case "OnlyCommentsAndFormatting":
            helpText = "Specifies that only comments and <br /> formatting are shown in balloons.";
            break;
    }

    notifyId = SP.UI.Notify.addNotification(helpText, false);
}

function CompatibilityHelp(ddl) {
    var helpText = "";

    var compatibilityDdl = document.getElementById(ddl);
    var value = compatibilityDdl.options[compatibilityDdl.selectedIndex].value;

    if (notifyId != '') {
        RemoveHelp();
    }

    switch (value) {
        case "Word2003":
            helpText = "Convert the file to Word 97 to Word 2003 compatibility mode.";
            break;
        case "Word2007":
            helpText = "Convert the file to Word 2007 compatibility mode.";
            break;
        case "Word2010":
            helpText = "Convert the file to Word 2010 compatibility mode.";
            break;
        case "Word2013":
            helpText = "Convert the file to Word 2013 compatibility mode.";
            break;
        case "MaintainCurrentSetting":
            helpText = "Maintain the current compatibility mode setting specified by the file.";
            break;
        case "Current":
            helpText = "Convert the file to the most recent version.";
            break;
    }

    notifyId = SP.UI.Notify.addNotification(helpText, false);
}

function RemoveHelp() {
    SP.UI.Notify.removeNotification(notifyId);
    notifyId = '';
}

function Cancel() {
    window.frameElement.commonModalDialogClose(0);
}