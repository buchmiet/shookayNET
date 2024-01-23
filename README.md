# **Shookay Search Engine NET wrapper v0.6.1**

## **Overview**
ShookayNET is net wrapper for Shookay engine. Shookay is a open source high-performance search engine library designed to offer efficient and dynamic search capabilities. Shookay NET enables you to use shookay with NET classes.

## **Features**
- Fast Performance: shookay is based on Inverted Search algorithm;
- You can integrate it with your NET application and use your C# classes to search through;

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

        public class Car
        {
            public int Id { get; set; }
            public string brand { get; set; }
            public string model { get; set; }
            public int year { get; set; }
        }


        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public List<Address> Addresses { get; set; }
            public List<Car> Cars { get; set; }
        }
```

Now fill them with some data:
```cs
   var car1 = new Car
            {
                Id = 1,
                brand = "Ford",
                model = "Fiesta",
                year = 1997
            };
            var car2 = new Car
            {
                Id = 2,
                brand = "Mercedez",
                model = "A200",
                year = 1980
            };
            var car3 = new Car
            {
                Id = 3,
                brand = "Toyota",
                model = "Camry",
                year = 2010
            };
            var car4 = new Car
            {
                Id = 4,
                brand = "Renault",
                model = "Megane",
                year = 2008
            };
            var car5 = new Car
            {
                Id = 5,
                brand = "Opel",
                model = "Astra",
                year = 2015
            };
            var car6 = new Car
            {
                Id = 6,
                brand = "Opel",
                model = "Astra",
                year = 2015
            };

            var adr1 = new Address
            {
                Id = 1,
                streetname = "main street",
                streetNumber = 2
            };
            var adr2 = new Address
            {
                Id = 2,
                streetname = "elm street",
                streetNumber = 23
            };
            var adr3 = new Address
            {
                Id = 3,
                streetname = "ulica Lotników",
                streetNumber = 4
            };
            var adr4 = new Address
            {
                Id = 4,
                streetname = "Bahnhofstraße",
                streetNumber = 17
            };
            var adr5 = new Address
            {
                Id = 5,
                streetname = "Carrer del Dr. Vicente Pallarés",
                streetNumber = 46
            };

            var person1 = new Person
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Addresses= [adr1, adr3],
                Cars = [car3,car1]
                
            };
            var person2 = new Person
            {
                Id = 2,
                Name = "John",
                Surname = "Smith",
                Addresses = [adr2],
                Cars = [car1,car4]
            };
            var person3 = new Person
            {
                Id = 3,
                Name = "Jürgen",
                Surname = "Schmidt",
                Addresses = [adr4,adr5],
                Cars = [car6,car2]
            };
            var person4 = new Person
            {
                Id = 4,
                Name = "Juan",
                Surname = "Herrero",
                Addresses = [adr2, adr5],
                Cars = [car2, car3,car5]
            };
            List<Person> lista = [person1,person2,person3,person4];
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
ID:3, Name:Jürgen Surname:Schmidt
ID:4, Name:Juan Surname:Herrero
```

**Customization**

ShookayNET scans your objects and extracts all the string properties associated with your objects. This may be insufficient or may deliver too many results. For instance, let's say that in our example you want to car year to be searchable. As this propert is integer, it will not be automatically counted as text. You will need to write your own object parser and deliver it to the engine.

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
            foreach (var item in person.Cars)
            {
                retString.Append(item.model);
                addSpace();
                retString.Append(item.brand);
                addSpace();
                retString.Append(item.year);
                addSpace();
            }

            return new KeyValuePair<int, string> (person.Id,retString.ToString());
            void addSpace()
            {
                retString.Append(' ');
            }
        }
```

Above method will include both street number and car's year pf manufacture. This method will generate the following string for object person1:

```
Jan Kowalski main street 2 ulica Lotników 4 Camry Toyota 2010 Fiesta Ford 1997
```

You will have to inform the search engine that it should your object processor when creating its instance:

```cs
var es = new ShookayWrapper<Person>(lista, PersonDataExtractor);
```

And you are ready to go. Now, you are looking for a person that has a car manufactured in 1997:

```cs
var results =await es.FindExact("1997");
```

And the results:

```
ID:1, Name:Jan Surname:Kowalski
ID:2, Name:John Surname:Smith
```

Voila!


#Adding new entries :

```cs
var person5 = new Person
{
    Id = 4,
    Name = "Ian",
    Surname = "Black",
    Addresses = [adr1, adr5],
    Cars = [car5]
}
await es.AddEntry(person5);
```


Object will be parsed and added to your dictionary


## Usage with GUI

First create a method that matches the following delegate :

```cs
 public delegate void ProgressCallback(float progress);
```

To deliver your entries :

```cs
public static void PrintResults(int progress)
{
      Console.WriteLine($"match at: {progress}%");
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

# [0.5.1] - 2024-01-09
added:
 ```DeliverEntriesWithCallback``` methods that can report progress to your GUI
# [0.5.2] - 2024-01-10
added:
 ```FindExactWithProgress``` and ```FindWithinWithProgress``` methods that can report progress to your GUI
# [0.6.1] - 2024-01-19
<<<<<<< HEAD
Major breaking changes to the API
# [0.6.2] - 2024-01-23
Added AddEntry Method
=======
 Major breaking changes to the API
>>>>>>> 71470bc52fe097223b77dde9cd156e08bdfbe1ff
