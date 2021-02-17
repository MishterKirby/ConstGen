- - - -

#### If you like this asset, a donation of any value is very much appreciated [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/donate?hosted_button_id=RTBZPSYEFNUGG)

- - - -

# Const Generator
Unity Constants Generator

![ConstGen Window](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/ConstGen.PNG)

[ Tested on 2019.4.15f ]

#### Const Generator generates constant properties within static classes replacing the usage of magic strings by holding the value of those strings in the given constant property. ####
.<br/>
.<br/>
.<br/>
.<br/>
.<br/>
but why the use for this, you ask?<br/>
why not just use magic strings and simply write it like this:<br/>

 `animator.SetFloat("XMovement", 5);`ノ( ゜-゜ノ)

Well....<br/>
Because screw this! (╯°□°）╯︵ `;(ϛ ',,ʇuǝɯǝʌoWX,,)ʇɐolℲʇǝS˙ɹoʇɐɯᴉuɐ`

<br/>

Magic strings are considered bad, you can use them sparingly but no more than that. They are very error-prone and hard to fix if any problems had arisen. 
So instead of typing out the string directly as an argument to a method, the constant property can be used.

![usage example](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/example%20use.png)

By this way you are sure there is no typos on the string you are passing. <br/>
Plus you got some of that neat pop up suggestions on other related constant properties and auto-completion :ok_hand:

- - - -

## Features ##

### ( Constants Generation ) ###

#### ConstGen can generate the type of unity constants for: ####
- [x] Layers
- [x] Tags
- [x] Sorting Layers
- [x] Shader Properties
- [x] Animator Controller Parameters
- [x] Animator Controller Layers
- [x] Animator Controller States
- [x] Nav Mesh Areas

<br/>

### ( Constant Generator Creation ) ###
#### ConstGen can also create generator scripts ####
![generator creation](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/generator%20creation.png)

- - - -

## Usage ##

### ( Generating Constants ) ###
![generating constants](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/Generating%20Constants.png)

### Settings ###

**[ReGen On Missing]** - Sets the generator to generate it's constants file if it detected none exists. <br/>
NOTE: the [Force Generate] button depends on this setting as it will delete the constants file and let the generator create a new one.

**[Update On Reload]** - Sets the generator to automatically generate/update it's constants file on editor recompile if any changes is detected within the unity editor, e.g adding new layers or deleting animator controller paramters.

NOTE: All generator update checks are are done upon editor recompile so the generator won't trigger script generate and recompile every after little change you want on the editor constants. 

### Generation ###

**[Generate]** - Updates the type of constants or generates the file is none is present.

**[Force Generate]** - Deletes the file on the type of constants and let the generator regenerate a new one. 

**[Generate ALL] & [Force Generate ALL]** - you know.....just like the generate & force generate buttons but instead triggers all generators.

### Constants Files/Generated Files ###
![generating constants](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/generated%20files.PNG)

Constants files are generated at (ConstGen/Generated Files) directory.

NOTE: Don't move around the files inside the ConstGen folder it will break the generators but you can move the ConstGen folder itself at any Directory in the Assets.

ANOTHER NOTE: In the event of for some reason the generated files has an error and [Force Generate] won't delete the file, you can manually delete the file itself in the it's folder with the **[ReGen On Missing]** turned on and the generator/s will try to generate a new file.


### ( Creating Generators ) ###
![generator creation](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/creating%20generators.PNG)

You can create generator scripts like the ones ConstGen use to generate the constants properties you want through script. 

**Generate Name** - Already self explanatory, this will also be the name of the generator script.

**Output File Name** - The name of the generated file by the generator which is also the generated file's script name.

Created generators are generated in (ConstGen/Editor/Generated Generators)

- - - -

## Using The Constants ##
![generator creation](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/usage.png)

Import the `ConstGenConstants` namespace on which the constants are in and from there you can access them.
