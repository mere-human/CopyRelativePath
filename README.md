# Copy Relative Path Extension

![document tab menu](Resources/file-path_icon-icons.com_71653_128.png)

Download: [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=mere-human.CopyRelativePath)

## Description

This Visual Studio extension adds advanced path copying commands. Available commands:

1. **Copy Relative Path** - Get a relative path to the document. This might be useful when adding a C++ include statement. _Example:_ transform `D:\notepad-plus-plus\PowerEditor\src\resource.h` into `PowerEditor/src/resource.h`.

2. **Copy With Prefix** - Append some text to the relative document path. This might be used to get a GitHub link to the selected document in Visual Studio.
_Example:_ transform `D:\notepad-plus-plus\PowerEditor\src\resource.h`
into `https://github.com/notepad-plus-plus/notepad-plus-plus/blob/master/PowerEditor/src/resource.h`.

The new commands are available in the context menu for a document tab:

![document tab menu](Resources/menu-doc-tab.png)

Or in the context menu for an item in the Solution Explorer:

![solution explorer menu](Resources/menu-solution-explorer.png)

Settings can be customized in _Tools > Options > Copy Relative Path Extension_:

![options dialog](Resources/options-dialog.png)

## Links

* File path icon by [Picol](https://icon-icons.com/icon/file-path/71653), [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)