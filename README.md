# **Shookay Search Engine NET wrapper v0.5.1**

## **Overview**
ShookayNET is net wrapper for Shookay engine. Shookay is a open source versatile, high-performance search engine library designed to offer efficient and dynamic search capabilities. Shookay NET enables you tyou use shookay with NET objects. It can be used for web api where results need to be provided instantly or it can provide results on the go.

## **Features**
- Fast Performance: Optimized with C++ and x64 assembly, Shookay ensures rapid search results even with large datasets.
- Flexible Search Options: Offers both 'FindWithin' and 'FindExact' search methods to cater to different search requirements.
- Multilingual Support: Compatible with UTF-8, UTF-16, and UTF-32 encoding, providing versatility for international applications.
- Dynamic Search Capabilities: Allows for searching by partial word fragments and associated terms, enhancing user experience.
- Separate Search Engine Instances: Each application component can utilize its own instance of the search engine for specialized data handling.

## **Installation and Integration**

**Clone the Repository:** 

Clone or download the shookay repository from GitHub.

git clone https://github.com/buchmiet/shookayNET 

**Add shookayNET to your Project:** 

Command Line:

```dotnet sln [solution-name].sln add [path-to-shookayNET-project]\shookayNET.csproj```

Visual Studio:

Open your solution and click on "Add existing  project". Choose shookayNET.sln


**Usage**

add 

```cs
using shookayNET;
```

to your project

**Initializing the search dictionary**

Let's assume you have the following objects:

```cs
  public class Address
  {
      public int Id { get; set; }
      public string streetname { get; set; }
      public int streetNumber { get; set; }
  }

  public class Person
  {
      public int Id { get; set; }
      public string Name { get; set; }
      public string Surname { get; set; }
      public List<Address> Addresses { get; set; }
  }
```

Now fill them with some data:
```cs
 var adr1 = new Address
 {
     Id = 2,
     streetname = "main street",
     streetNumber = 2
 };
 var adr2 = new Address
 {
     Id = 2,
     streetname = "elm street",
     streetNumber = 2
 };
 var person1 = new Person
 {
     Id = 1,
     Name = "Jan",
     Surname = "Kowalski",
     Addresses= [adr1, adr2]
 };

 var adr3 = new Address
 {
     Id = 3,
     streetname = "lorne street",
     streetNumber = 2
 };
 var adr4 = new Address
 {
     Id = 4,
     streetname = "central street",
     streetNumber = 26
 };
 var person2 = new Person
 {
     Id = 2,
     Name = "John",
     Surname = "Smith",
     Addresses = [adr3, adr4]
 };

 List<Person> lista = new List<Person>{person1,person2};
```

Now create the instance of the engine:

```cs
var es = new ShookayWrapper<Person>(lista);
```

Now, tell the engine to prepare the data. If the dataset is large, you may want to run it on separate thread:

```cs
await es.DeliverEntries();
```

That's it, search engine is ready. 

If you are looking for an object that has string "Jan" in it :

```cs
   var results =await es.FindExact("Jan");
   foreach (var result in results)
   {
       var person = lista.First(p => p.Id == result);
       Console.WriteLine($"ID:{person.Id}, Name:{person.Name} Surname:{person.Surname}");
   }
```

Results will be 

```
ID:1, Name:Jan Surname:Kowalski
```

You may be also looking for an object that has string "str" being part of words within:

```cs
var results =await es.FindWithin("str");
foreach (var result in results)
{
    var person = lista.First(p => p.Id == result);
    Console.WriteLine($"ID:{person.Id}, Name:{person.Name} Surname:{person.Surname}");
}
```

Results will be 

```
ID:1, Name:Jan Surname:Kowalski
ID:2, Name:John Surname:Smith
```

**Customization**

ShookayNET scans your objects and extracts all the string properties associated with your objects. This may be insufficient or may deliver too many results. For instance, let's say that in our example you want to include street number to be searchable. As this propert is integer, it will not be automatically counted as text. You will need to write your own object parser and deliver it to the engine.

Object parser must match the following criteria :

```cs
public KeyValuePair<int, string> ObjectToEntry(T obj)
```

So for the above example you will create the following method:

```cs
 public static KeyValuePair<int, string> PersonDataExtractor(Person person)
 {
     StringBuilder retString = new StringBuilder();
     retString.Append(person.Name);
     addSpace();
     retString.Append(person.Surname);
     addSpace();                      
     foreach (var item in person.Addresses)
     {
         retString.Append(item.streetname);
         addSpace();
         retString.Append(item.streetNumber);
         addSpace();
     }
     return new KeyValuePair<int, string> (person.Id,retString.ToString());
     void addSpace()
     {
         retString.Append(' ');
     }
 }
```

Above method will include street number. This method will generate the following string for object person1:

```
Jan Kowalski main street 2 elm street 2
```

You will have to iinform the search engine that it should your object processor when creating its instance:

```cs
var es = new ShookayWrapper<Person>(lista, PersonDataExtractor);
```

And you are ready to go. Now, you are looking for an object that has number 26 in it :

```cs
var results =await es.FindExact("26");
```

And the results:

```
ID:2, Name:John Surname:Smith
```

Voila!

## Usage with GUI

First create a method that matches the following delegate :

```cs
public delegate void ProgressCallback(int progress);
```

for example

```cs
public static void PrintProgress(int progress)
{
    Console.WriteLine($"Progress: {progress}%");
}
```

Then deliver your objects 

```cs
await es.DeliverEntriesReportProgress(PrintProgress);
```

Result:
```
Progress: 0%
Progress: 50%
```

## **License**

This project is licensed under the MIT License - see the LICENSE file for details.

### MIT License Summary

The MIT License is a permissive license that is short and to the point. It lets people do anything they want with your code as long as they provide attribution back to you and don’t hold you liable.

- Permissions
- Commercial use
- Modification
- Distribution
- Private use
- Conditions
- Include the original license and copyright notice with the code
- Limitations
- No Liability
- No Warranty

### What's new:

[0.5.1] - 2024-01-09
added:
 DeliverEntriesWithCallback methods that can report progress to your GUI
