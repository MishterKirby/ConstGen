# Const Generator
Unity Constants Generator

![ConstGen Window](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/ConstGen.PNG)

#### Const Generator generates constant properties within static classes replacing the usage of magic strings by holding the value of those strings in the given constant property. ####
.<br/>
.<br/>
.<br/>
.<br/>
.<br/>
but why the use for this, you ask?<br/>
why not just use magic strings and simply write it like this:<br/>

 `animator.SetFloat("Speed", 5);`ノ( ゜-゜ノ)

Well....<br/>
Because screw this! (╯°□°）╯︵ `;)5 ,"deepS"(taolFteS.rotamina`

<br/>

Magic strings are considered bad, you can use them sparingly but no more than that. They are very error-prone and hard to fix if any problems had arisen. 
So instead of typing out the string directly as an argument to a method, the constant property can be used.

![usage example](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/example%20use.png)

By this way you are sure there is no typos on the string you are passing. <br/>
Plus you got some of that neat pop up suggestions on other related constant properties and auto-completion :ok_hand:

- - - -

## Features ##

### [ Constants Generation ] ###
#### ConstGen can generate the type of unity constants for: ####
- [x] Layers
- [x] Tags
- [x] Sorting Layers
- [x] Shader Properties
- [x] Animator Controller Parameters
- [x] Animator Controller Layers
- [x] Animator Controller States
- [x] Nav Mesh Areas

### [ Constant Generator Creation ] ###

#### ConstGen can also create generator scripts ####

![generator generation](https://github.com/INFGameDev/Project-ReadMe-Images/blob/master/ConstGen/create%20generator.PNG)

You can create generator scripts like the ones ConstGen use to generate the constants properties you want through script. 
