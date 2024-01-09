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

Create instance of the engine:

```cpp
shookayEngine* searchEngine = CreateSearchEngine();
