# ObjectDataStructure C# (ODSSharp)
Object Data Structure is a file format inspired by NBT. Everything in this file format is made of tags.
ODS is not human readable, data is stored in bytes.  
  
This is the offical C# port of the [Java version of ODS](https://github.com/ryandw11/ODS). The API is kept as true to the Java version as possible while still compling with C# standards.  
  
**ODSSharp is still in development and is not perfect yet. The API could change from update to update.**
  
The documentation of ODSSharp is not complete yet. You can use the [Java Documentation](https://ryandw11.github.io/ODS/) instead. It is almost the same besides the fact that methods are camel cased in Java.

# Usage
As stated above ODS uses tags. There are many primative tags: StringTag, IntTag, ShortTag, LongTag, ByteTag, DoubleTag, FloatTag.
There are also the ListTag and DirectoryTag. They both store primative tags in a list and directory format respectivly.
Finally there are ObjectTags. ObjectTags store other tags. For more information about the possibilites be sure to check out the wiki and/or the [Java Documentation](https://ryandw11.github.io/ODS/)!  
  
You will need to use the following:
```c#
using ODS;
using ODS.tag;
// If you want to use the class serialization V
using ODS.Serializer;
```

## ODSUtil Utility Class
The ODS class is full of useful methods that allow the easy serialization of primative objects.  
The ODS class also allows the serialization of custom objects.  
  
**Note: Due to the nature of C# and generics, the Utility class is not as useful/functional as the Java version. Lists cannot be serialized in classes for now**.
## Example for saving
```c#
ObjectDataStructure ods = new ObjectDataStructure(new FileInfo(Directory.GetCurrentDirectory() + "\\test3.ods"));

List<ITag> tags = new List<ITag>();
tags.Add(new StringTag("ExampleKey", "This is an example string!"));
tags.Add(new IntTag("ExampleInt", 754));

ObjectTag car = new ObjectTag("Car");
car.AddTag(new StringTag("type", "Jeep"));
car.AddTag(new IntTag("gas", 30));
List<IntTag> coordsList = new List<IntTag>() { new IntTag("", 10), new IntTag("", 5), new IntTag("", 10) };
car.AddTag(new ListTag<IntTag>("coords", coordsList));

ObjectTag owner = new ObjectTag("Owner");
owner.AddTag(new StringTag("firstName", "Jeff"));
owner.AddTag(new StringTag("lastName", "Bob"));
owner.AddTag(new IntTag("Age", 30));
car.AddTag(owner);

tags.Add(car);

ods.Save(tags);
```

## Example for loading
```C#
StringTag tag = (StringTag) ods.Get("ExampleKey");
Console.WriteLine("The value of the ExampleKey is: " + tag.GetValue());

ObjectTag myCar = (ObjectTag) ods.Get("Car");
StringTag myCarType = (StringTag)myCar.GetTag("type");
Console.WriteLine("The car is a " + myCarType.GetValue());

StringTag ownerFirstName = (StringTag) ods.GetObject("Car.Owner.firstName");
StringTag ownerLastName = (StringTag)ods.GetObject("Car.Owner.lastName");
Console.WriteLine("The owner of the car is " + ODSUtil.UnWrap(ownerFirstName) + " " + ODSUtil.UnWrap(ownerLastName));
```
# ODS Visualizer
This tool allows you inspect ods files. The tool is coded in Java so you will need to install Java to use it. 
![Picture Of the Visualizer](https://i.imgur.com/ukROPZy.png)  
[Click here to go to the visualizer repository.](https://github.com/ryandw11/ODS_Visualizer)

# Offical Language Ports
 - [ODS (Java)](https://github.com/ryandw11/ODS)
 - C++ (Coming Soon)
 
 # ODS Specifications
 The specifications for the ODS format can be found [here on the main respository](https://github.com/ryandw11/ODS#specification)
 
# Contributing to the project
Feel free to contribute any bug fixes or performance optimizations to this repository.  
Any changes to the API must be suggested [on the main repository.](https://github.com/ryandw11/ODS)  

# Dependencies 
This project uses [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) for ZLib compression. Their code is licensed under `MIT`.
