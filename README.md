# SearchLinks
A tool for search links in your computer.

# Quick Start

Use following command to search all directory links under `C:/`:
```shell
SearchLinks C:/
```

# Guide

* Use ```shell
SearchLinks --help
``` to get help document.

* Use ```shell
SearchLinks <path>
``` to search links under "path". (Scaning including "path")

* Internal working principle: It will scanning ever directory's attribute to comfirm whether it is a link. If it is a link, the program won't keep scaning its sub-directories.


# Application Cases

* If you forget where you made an link, you can use this program to find out them.

# Advantages

* Easy to use.

# Disadvantages

* Single thread.

* Only support directory links.

* Non-support Windows link file.

* Low customization operation.

# Attention

* Those code was only tested on `Windows 10`.
