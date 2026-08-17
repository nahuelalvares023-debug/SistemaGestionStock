# SistemaGestionStock

API REST desarrollada con C# y .NET para la gestión de stock de productos.

El sistema permite crear categorías y productos, registrar movimientos de stock (entradas y salidas) con auditoría completa, detectar productos con stock por debajo del mínimo, y exportar/importar el inventario completo mediante archivos Excel.

## Tecnologías utilizadas

- C#
- .NET / ASP.NET Core
- Entity Framework Core
- SQL Server
- ClosedXML (lectura y escritura de archivos Excel)
- Swagger / OpenAPI
- Git / GitHub

## Funcionalidades

- Gestión de categorías (CRUD completo).
- Gestión de productos (CRUD completo).
- Registro de movimientos de stock (entradas y salidas) con motivo y fecha.
- El stock **no se edita directamente**: solo cambia a través de movimientos, dejando un historial auditable de cada modificación.
- Validación de stock insuficiente al registrar una salida.
- Endpoint de alerta para productos con stock por debajo del mínimo definido.
- Exportación del listado de productos a un archivo `.xlsx`.
- Importación de productos desde un archivo `.xlsx`, con validación fila por fila (los errores en una fila no interrumpen el procesamiento del resto).
- Persistencia de datos con SQL Server mediante Entity Framework Core.
- Migraciones mediante Entity Framework Core.
- Documentación y pruebas de la API mediante Swagger/OpenAPI.

## Arquitectura

El proyecto utiliza una estructura basada en controladores y Entity Framework Core:

```
StockApi/
├── Controllers/
├── Data/
├── Migrations/
├── Models/
├── Properties/
├── Program.cs
├── appsettings.json
└── StockApi.csproj
```

### Principales componentes

**Controllers**
Contienen los endpoints de la API y la lógica relacionada con las solicitudes HTTP.

**Models**
Contienen las entidades utilizadas por el sistema: `Categoria`, `Producto` y `MovimientoStock`.

**Data**
Contiene el `AppDbContext`, encargado de la comunicación entre la aplicación y SQL Server.

**Migrations**
Contiene las migraciones generadas mediante Entity Framework Core para crear y actualizar la estructura de la base de datos.

**Program.cs**
Configura los servicios principales de la aplicación, incluyendo Entity Framework Core, SQL Server, Swagger y controladores.

## Modelo de datos

El sistema está compuesto por tres entidades relacionadas:

- **Categoria** — agrupa productos (relación uno a muchos con `Producto`).
- **Producto** — pertenece a una categoría y tiene un stock actual y un stock mínimo.
- **MovimientoStock** — registra cada entrada o salida de stock de un producto, con cantidad, motivo y fecha (relación uno a muchos con `Producto`).

## Base de datos

El proyecto utiliza SQL Server mediante Entity Framework Core.

La conexión se configura en `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=StockApiDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

La base de datos utilizada por el proyecto es `StockApiDb`.

### Migraciones

Para aplicar las migraciones existentes y crear/actualizar la base de datos:

```
dotnet ef database update
```

Para crear una nueva migración:

```
dotnet ef migrations add NombreDeLaMigracion
```

## Ejecutar el proyecto

1. Clonar el repositorio:
```
git clone https://github.com/nahuelalvares023-debug/SistemaGestionStock.git
```

2. Ingresar a la carpeta del proyecto:
```
cd SistemaGestionStock/StockApi/StockApi
```

3. Restaurar las dependencias:
```
dotnet restore
```

4. Ajustar la cadena de conexión en `appsettings.json` según el nombre de tu instancia de SQL Server.

5. Aplicar las migraciones:
```
dotnet ef database update
```

6. Ejecutar la API:
```
dotnet run
```

Una vez iniciada la aplicación, Swagger permite visualizar y probar los endpoints disponibles en `/swagger`.

## Endpoints

### Categorías — `/api/Categorias`

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/Categorias` | Obtiene todas las categorías |
| GET | `/api/Categorias/{id}` | Obtiene una categoría por ID |
| POST | `/api/Categorias` | Crea una nueva categoría |
| PUT | `/api/Categorias/{id}` | Actualiza una categoría existente |
| DELETE | `/api/Categorias/{id}` | Elimina una categoría |

Ejemplo de creación:
```json
{
  "nombre": "Electrónica"
}
```

### Productos — `/api/Productos`

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/Productos` | Obtiene todos los productos |
| GET | `/api/Productos/{id}` | Obtiene un producto por ID |
| POST | `/api/Productos` | Crea un nuevo producto |
| PUT | `/api/Productos/{id}` | Actualiza un producto (no modifica el stock) |
| DELETE | `/api/Productos/{id}` | Elimina un producto |
| GET | `/api/Productos/stock-bajo` | Lista los productos con stock por debajo del mínimo |

Ejemplo de creación:
```json
{
  "nombre": "Teclado Mecánico Redragon",
  "descripcion": "Teclado mecánico retroiluminado RGB, switches red",
  "precio": 62500.50,
  "stockActual": 22,
  "stockMinimo": 5,
  "categoriaId": 1
}
```

### Movimientos de stock

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/Productos/{id}/entrada` | Registra una entrada de stock |
| POST | `/api/Productos/{id}/salida` | Registra una salida de stock (valida stock suficiente) |
| GET | `/api/Productos/{id}/movimientos` | Obtiene el historial de movimientos de un producto |

Ejemplo de entrada/salida:
```json
{
  "cantidad": 20,
  "motivo": "Compra a proveedor"
}
```

Si se intenta registrar una salida con una cantidad mayor al stock disponible, la API devuelve un error `400` indicando el stock actual.

### Excel — `/api/Productos`

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/Productos/exportar` | Descarga un archivo `.xlsx` con todos los productos |
| POST | `/api/Productos/importar` | Crea productos a partir de un archivo `.xlsx` subido |

El archivo de importación debe respetar el mismo formato que genera la exportación: `Id, Nombre, Descripcion, Precio, StockActual, StockMinimo, CategoriaId`. Las filas con errores (categoría inexistente, datos faltantes, etc.) se omiten sin interrumpir el procesamiento del resto, y la respuesta incluye un detalle de los errores encontrados.

## Códigos HTTP utilizados

| Código | Significado |
|---|---|
| 200 OK | Solicitud procesada correctamente |
| 201 Created | Recurso creado correctamente |
| 204 No Content | Operación realizada sin contenido de respuesta |
| 400 Bad Request | Solicitud inválida (datos faltantes, stock insuficiente, etc.) |
| 404 Not Found | Recurso no encontrado |

## Objetivo del proyecto

El objetivo del proyecto es desarrollar una API REST utilizando tecnologías habituales en aplicaciones backend profesionales, aplicando conceptos de:

- Desarrollo de APIs REST.
- Modelado de relaciones entre entidades.
- Persistencia de datos y migraciones con Entity Framework Core.
- Diseño de lógica de negocio (control de stock, auditoría de movimientos).
- Manejo de archivos (lectura y escritura de Excel).
- Documentación mediante Swagger.
- Control de versiones con Git y GitHub, incluyendo trabajo sincronizado entre distintos equipos.
