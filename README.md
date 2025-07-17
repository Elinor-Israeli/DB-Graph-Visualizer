# DB Graph Visualizer

## 🚀 Features

- Visualizes a relational database schema as a graph (Support only Microsoft SQL Server).
- Uses MSAGL for automatic layout and rendering.
- Custom node styling for tables (name, primary/foreign keys).
- Edges with tooltip support.
- Custom tooltip rendering (e.g., multiplicity: 1..*, 0..1, etc).
- Exportable views (images).
- Fully written in C# using Windows Forms / WPF.

## How To Run

1. Replace the `ConnectionString` variable in `DBGraphVisualizer\Services\SchemaLoader.cs` with your Database URL
2. Run the DBGraphVisualizer csproj

## 📸 Screenshots

### Screenshots are of AdventureWorks sample database

 <img width="1901" height="404" alt="image-1" src="https://github.com/user-attachments/assets/9241f79c-d0e8-4cb1-b5d1-c2401a14626e" />
 <img width="1898" height="983" alt="image-2" src="https://github.com/user-attachments/assets/23e712d8-8a55-408e-a437-a2ea2034c761" />
 <img width="1910" height="982" alt="image-3" src="https://github.com/user-attachments/assets/34180e37-89e1-48cb-8f6f-215234f55c30" />

## 🛠️ Technologies

- C#
- .NET Framework
- Windows Forms / WPF
- Microsoft.Msagl (Graph layout)
- SQL Server (AdventureWorks sample DB)
