# **Shookay Search Engine NET wrapper v0.5.0**

## **Overview**
ShookayNET is net wrapper for Shookay engine. Shookay is a open source versatile, high-performance search engine library designed to offer efficient and dynamic search capabilities. Shookay NET enables you tyou use shookay with NET objects.

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
 var person = new Person
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

 List<Person> lista = new List<Person>{person,person2};
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
