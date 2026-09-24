EVOLUTIONSIM — FASE 8.1
MORPHOLOGY FOUNDATION
=====================

OBJETIVO
--------
Iniciar la Fase 8 creando una representación morfológica heredable,
determinista y observable.

En esta fase la morfología es COMPLETAMENTE PASIVA.

No modifica:

- energía
- metabolismo
- temperatura
- velocidad
- movimiento
- migración
- alimentación
- caza
- carroñeo
- reproducción
- mating
- supervivencia
- especies

PRINCIPIO
---------
La morfología no se genera con Random adicional.

Se deriva exclusivamente del genoma existente.

Esto permite:

- reproducibilidad exacta;
- herencia mediante los loci estructurales;
- divergencia morfológica gradual;
- futura conexión con ecología;
- futura visualización del organismo.

CUERPO BASE
-----------
BodyScale utiliza directamente:

Genome.Size

No se redefine Size.

No se modifica la fórmula histórica.

CANALES MORFOLÓGICOS ESTRUCTURALES
----------------------------------
Se introducen cinco canales:

1. LimbLength
   Longitud relativa de extremidades.

2. LimbRobustness
   Robustez estructural de extremidades.

3. Insulation
   Desarrollo relativo de estructuras de aislamiento corporal.

4. JawStrength
   Desarrollo estructural mandibular.

5. DigestiveStructure
   Desarrollo relativo de estructuras digestivas.

En Fase 8.1 estos nombres describen morfología potencial.
Todavía no implican ninguna ventaja ecológica.

RANGO
-----
Cada canal estructural se expresa como:

-0.08 .. +0.08

y también puede consultarse como factor:

0.92 .. 1.08

MAPEO DE LOCI
-------------
Los loci estructurales introducidos en Fase 7.2 se reutilizan.

Cada FamilyId se asigna determinísticamente a un MorphologyChannel.

IMPORTANTE:

NO se modifica:

GenomePhenotypeMapper.ResolveChannel()

El mapeo morfológico utiliza una SAL distinta antes del hash.

Por tanto:

- el phenotype de Fase 7 permanece exactamente igual;
- no cambian los canales fenotípicos existentes;
- no cambia el RNG;
- no cambia la herencia;
- no cambia la ecología.

PLEIOTROPÍA
-----------
Un mismo locus estructural puede participar:

- en un canal fenotípico de Fase 7;
- y en un canal morfológico de Fase 8.

Pero ambos mapeos son independientes.

Esto introduce una base para pleiotropía estructural sin cambiar todavía
la selección, porque la morfología continúa siendo pasiva.

NUEVAS CLASES
-------------
EvolutionSim/Models/MorphologyChannel.cs
EvolutionSim/Models/MorphologyProfile.cs
EvolutionSim/Models/GenomeMorphologyMapper.cs
EvolutionSim/Evolution/MorphologyStatisticsCalculator.cs

ARCHIVOS MODIFICADOS
--------------------
EvolutionSim/Models/Organism.cs
EvolutionSim/Simulation/SimulationStepResult.cs
EvolutionSim/Simulation/SimulationEngine.cs

ORGANISM
--------
Se agrega:

Morphology

La propiedad se calcula determinísticamente desde Genome.

No consume Random.

DIAGNÓSTICOS
------------
Se agrega telemetría global y regional:

- población viva;
- organismos con expresión morfológica estructural;
- BodyScale promedio;
- magnitud estructural promedio;
- promedio por canal;
- desviación estándar;
- mínimo;
- máximo;
- cantidad de organismos expresando cada canal.

SIMULATIONSTEPRESULT
--------------------
Se agrega:

MorphologyDiagnostics

Esto prepara la morfología para:

- Desktop;
- snapshots;
- replay;
- renderer futuro;
- análisis regional;
- futuras conexiones ecológicas.

PROGRAM.CS
----------
NO CAMBIA.

SIMULATIONCONFIG.CS
-------------------
NO CAMBIA.

PHENOTYPEPHASE7PROFILE.CS
------------------------
NO CAMBIA.

NUGET
-----
Ninguno.

VALIDACIÓN
----------
Compilar:

dotnet build -c Release

Ejecutar:

dotnet run -c Release --project .\EvolutionSim\EvolutionSim.csproj

Como la morfología es 100 % pasiva, seed 98765 debe conservar
EXACTAMENTE el cierre de Fase 7:

C1600
Población: 1971
Valle: 369
Bosque: 917
Llanura: 685
Especies: 2

Además debe aparecer una sección:

Morfologia

con:

Expresados
BodyScale
StructMag

y estadísticas de:

LimbLength
LimbRobustness
Insulation
JawStrength
DigestiveStructure

CRITERIO DE ÉXITO
-----------------
Fase 8.1 queda validada únicamente si:

1. el baseline ecológico permanece exacto;
2. los diagnósticos morfológicos aparecen;
3. hay variación estructural en generaciones avanzadas;
4. no aparece ninguna llamada nueva a Random;
5. los mismos genomas producen siempre la misma morfología.

SIGUIENTE PASO
--------------
Después de validar 8.1:

Fase 8.2 — Morfología por especie y divergencia morfológica.

Todavía sin activar efectos ecológicos.

Primero queremos responder:

¿las poblaciones/especies que ya divergen ecológicamente están
acumulando también diferencias corporales heredables?
