EVOLUTIONSIM — FASE 7.10
CIERRE Y ESTABILIZACIÓN DEL SISTEMA FENOTÍPICO
==============================================

OBJETIVO
--------
Cerrar formalmente la Fase 7 sin introducir biología nueva.

Esta fase:

- centraliza los couplings oficiales;
- documenta qué canales quedaron activos;
- documenta qué canales quedaron pasivos;
- conserva todos los mecanismos experimentales;
- conserva todos los runners experimentales;
- no cambia fórmulas ecológicas;
- no consume RNG adicional;
- no elimina diagnósticos.

NUEVA CLASE
-----------
EvolutionSim/Simulation/PhenotypePhase7Profile.cs

Esta clase se convierte en la única fuente de verdad para el perfil
fenotípico NORMAL al cierre de la Fase 7.

Valores oficiales:

MetabolicEfficiency = 0.125
ThermalRegulation   = 0.125
BodySize            = 0.0
Locomotion          = 0.0
FeedingSpecialization = 0.0

SIMULATIONCONFIG
----------------
SimulationConfig.Default ahora toma sus couplings oficiales desde:

PhenotypePhase7Profile

Esto evita dejar números mágicos repartidos entre propiedades y
documenta directamente en código el resultado de los estudios.

EXPERIMENTOS
------------
NO se eliminan.

Los runners de:

- MetabolicEfficiency
- ThermalRegulation
- BodySize
- Locomotion
- FeedingSpecialization

se conservan porque forman parte de la evidencia experimental y serán
útiles al ampliar la ecología en fases futuras.

Los runners pueden seguir sobrescribiendo los couplings mediante:

SimulationConfig with { ... }

sin alterar el perfil normal.

BIOLOGÍA
--------
NO CAMBIA.

MetabolicEfficiency:
continúa activo exactamente como estaba.

ThermalRegulation:
continúa activo exactamente como estaba.

BodySize:
su mecanismo experimental permanece en código, pero coupling = 0.

Locomotion:
su mecanismo experimental permanece en código, pero coupling = 0.

FeedingSpecialization:
su mecanismo experimental permanece en código, pero coupling = 0.

PROGRAM.CS
----------
NO CAMBIA.

SIMULATIONENGINE.CS
-------------------
NO CAMBIA.

ORGANISM.CS
-----------
NO CAMBIA.

PHENOTYPEECOLOGYCONTEXT.CS
--------------------------
NO CAMBIA.

GRAZINGSYSTEM.CS
----------------
NO CAMBIA.

SIMULATIONSTEPRESULT.CS
-----------------------
NO CAMBIA.

PAQUETES / NUGET
----------------
Ninguno.

VALIDACIÓN FINAL
----------------
1. Compilar:

dotnet build -c Release

2. Ejecutar la simulación normal:

dotnet run -c Release --project .\EvolutionSim\EvolutionSim.csproj

3. Para seed 98765 el cierre debe conservar el baseline validado:

C1600
Población: 1971
Valle: 369
Bosque: 917
Llanura: 685
Especies: 2

4. Confirmar los multiplicadores pasivos:

BodySize:
CapMult = 1.0000

Locomotion:
ActMult = 1.0000

FeedingSpecialization:
EnergyMult = 1.0000

5. Confirmar que continúan activos:

MetabolicEfficiency = 0.125
ThermalRegulation = 0.125

IMPORTANTE
----------
Los estudios de 7.9 ya comprobaron que el control de 10 seeds mantiene
el mismo estado de referencia que los experimentos previos.

Por tanto esta validación final no busca volver a seleccionar couplings.
Solo confirma que la centralización del perfil no produjo regresiones.

RESULTADO ESPERADO
------------------
Después de esta validación:

FASE 7 — COMPLETA

Siguiente fase:

FASE 8 — MORPHOLOGY
