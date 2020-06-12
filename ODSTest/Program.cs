﻿using System;
using System.Collections.Generic;
using System.IO;

using ODS;
using ODS.tags;

namespace ODSTest
{
    class Program
    {
        static void Main(string[] args)
        {
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

            // Loading Example

            StringTag tag = (StringTag) ods.Get("ExampleKey");
            Console.WriteLine("The value of the ExampleKey is: " + tag.GetValue());

            ObjectTag myCar = (ObjectTag) ods.Get("Car");
            StringTag myCarType = (StringTag)myCar.GetTag("type");
            Console.WriteLine("The car is a " + myCarType.GetValue());

            StringTag ownerFirstName = (StringTag) ods.GetObject("Car.Owner.firstName");
            StringTag ownerLastName = (StringTag)ods.GetObject("Car.Owner.lastName");
            Console.WriteLine("The owner of the car is " + ODSUtil.UnWrap(ownerFirstName) + " " + ODSUtil.UnWrap(ownerLastName));

        }
    }
}
