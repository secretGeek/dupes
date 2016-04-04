
Created by [Leon Bambrick](http://secretGeek.net)
withcontributions from Doeke Zanstra

# Dupes

Finds duplicate files, by calculating checksums.

Usage: 

    Dupes.exe [options]
    
    Options:

    -p, --path=VALUE           the folder to scan (defaults to current directory)
    -s, --subdirs              include subdirectories
    -f, --filter=VALUE         search filter (defaults to *.*)
    -a, --all                  show ALL checksums of files, even non-copies
    -?, -h, --help             show this message and exit



## How does it work?

For every member of a duplicate file set that the tool encounters, it spits out a row with four columns, separated by bar symbols ('|')

The four columns are:

* CheckSum:  Sha256 checksum of the file. (Hint: sort by this to get all duplicates together)
* DuplicateNum:  0 for the first file in the duplicate set, 1 for the second file, etc.
* Filesize:   In bytes. (Hint: sort by this, if you want to tackle big files first)
* Path:  Full path and filename for this duplicate.



## Tip

Redirect output to a `.csv` file, and manipulate the content with [NimbleText](http://NimbleText.com)


Here's an example pattern you could use in NimbleText for deleting all but the first copy of each file:

    <% if ($1 > 0) { 'del "' + $3 + '"' } %>


That pattern is just a piece of embedded javascript (you can embed javascript in NimbleText patterns) that says:

> if column 1 is greater than Zero, then output the text 'del ' plus the text from column 3.

Column 1 is the duplicate number, so it will be greater than zero for all but the first occurrence of each file. And column 3 is the full path and filename of the duplicate.




## Credits


 * Command line parsing uses the [Options class](https://github.com/mono/mono/blob/master/mcs/class/Mono.Options/Mono.Options/Options.cs) from [Mono](http://www.mono-project.com/)
 * Contributions from Doeke Zanstra
