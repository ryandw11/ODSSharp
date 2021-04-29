using System;
using System.Collections.Generic;
using System.IO;

using ODS;
using ODS.Tags;
using ODS.Compression;

namespace ODSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ObjectDataStructure ods = new ObjectDataStructure(new FileInfo(Directory.GetCurrentDirectory() + @"\test3.ods"), new GZIPCompression());
            // Register a custom tag.
            ODSUtil.RegisterCustomTag(new CustomTag("", ""));

            Console.WriteLine(Directory.GetCurrentDirectory() + "\\test3.ods");

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

            tags.Add(new CustomTag("Test", "Test"));

            CompressedObjectTag compressedObjectTag = new CompressedObjectTag("TestCompressedObject");
            compressedObjectTag.AddTag(ODSUtil.Wrap("TestObject", "This is a test!"));
            compressedObjectTag.AddTag(ODSUtil.Wrap("Number", 15));
            compressedObjectTag.AddTag(ODSUtil.Wrap("Decimal", 34.5));
            tags.Add(compressedObjectTag);


            ods.Save(tags);

            ods.Append(new StringTag("Test", "test"));

            ods.GetAll();

            // Loading Example

            StringTag tag = (StringTag) ods.Get("ExampleKey");
            Console.WriteLine("The value of the ExampleKey is: " + tag.GetValue());

            ObjectTag myCar = (ObjectTag) ods.Get("Car");
            StringTag myCarType = (StringTag)myCar.GetTag("type");
            Console.WriteLine("The car is a " + myCarType.GetValue());

            Console.WriteLine("First Name:");
            StringTag ownerFirstName = (StringTag) ods.Get("Car.Owner.firstName");
            Console.WriteLine("Last Name:");
            StringTag ownerLastName = (StringTag)ods.Get("Car.Owner.lastName");
            Console.WriteLine("The owner of the car is " + ODSUtil.UnWrap(ownerFirstName) + " " + ODSUtil.UnWrap(ownerLastName));
            Console.WriteLine(ods.Find("Car.Owner.firstName"));

            Console.WriteLine(((CustomTag)ods.Get("Test")).GetValue());

            ods.Set("Car.Owner.firstName", new StringTag("firstName", "Example"));
            ods.ReplaceData("Car.Owner.Age", new IntTag("Age", 3));
            Console.WriteLine(ODSUtil.UnWrap((IntTag)ods.Get("Car.Owner.Age")));

            CompressedObjectTag compressedObject = (CompressedObjectTag) ods.Get("TestCompressedObject");

            IntTag numberTag = (IntTag) compressedObject.GetTag("Number");
            Console.WriteLine("Test Compression Number: " + numberTag.GetValue());

            ods.SaveToFile(new FileInfo(@"new_file.ods"), new GZIPCompression());
            //ods.Set("Car.Owner.Age", new IntTag("Age", 3));

        }
    }
}
