# Automatización de datos ENSU para el Dashboard de Seguridad

Este proyecto utiliza información de la **Encuesta Nacional de Seguridad Pública Urbana (ENSU)** del INEGI para visualizar la percepción de inseguridad de la población mediante un dashboard interactivo.

El objetivo de la automatización es asegurar que el sistema siempre utilice **datos actualizados, oficiales y confiables**, sin depender de procesos manuales.

---

## ¿Por qué automatizar la obtención de los datos?

La ENSU se publica de forma periódica (marzo, junio, septiembre y diciembre).  
Si los datos se descargan manualmente, existe riesgo de:
- Errores humanos
- Información desactualizada
- Retrasos en el análisis

La automatización permite que el dashboard esté siempre listo para apoyar la **toma de decisiones estratégicas**.

---

## Proceso propuesto de automatización

### 1. Revisión automática de nuevos datos
Un proceso programado revisa periódicamente el portal del INEGI para verificar si existe un nuevo conjunto de datos ENSU publicado o actualizado.

Este proceso puede ejecutarse cada trimestre en el cual se suele hacer este reporte y estar atento a cualquier cambio de regla a futuro:

---

### 2. Descarga automática
Cuando se detecta un nuevo periodo disponible:
- El sistema descarga los archivos oficiales (ZIP o CSV).
- Los organiza por trimestre (Marzo, Junio, Septiembre, Diciembre).
- Se conserva el histórico para comparaciones en el tiempo.

---

### 3. Validación de la información
Antes de usar los datos:
- Se valida que los archivos no estén vacíos.
- Se comprueba que contengan las columnas necesarias (estado, municipio, sexo, percepción de inseguridad, etc.).
- Esto evita que el dashboard muestre información incorrecta.

---

### 4. Procesamiento de los datos
Los datos se preparan para su análisis:
- Se limpian valores inconsistentes.
- Se estandarizan nombres y categorías.
- Se calculan indicadores clave como los establecidos en la sección A o se ajusta a tener más KPI ya sea de manera prederteminada o flexible mediante flitros de busqueda más específicos.

---

### 5. Consumo por el dashboard
El dashboard consulta los datos ya procesados mediante una API o servicio interno.

Esto permite:
- Cambiar de estado o periodo en tiempo real.
- Visualizar comparaciones temporales.
- Mantener gráficos siempre actualizados.

---

## Beneficios para la Gerencia de Planeación

Este proceso de automatización permite:
- Identificar zonas de mayor riesgo de forma oportuna
- Apoyar la toma de decisiones basadas en datos
- Mejorar la planeación de políticas públicas
- Reducir tiempos operativos y errores manuales
- Garantizar el uso de datos oficiales del INEGI

---

## Resultado final

Cada vez que el INEGI publica nueva información ENSU:
- El sistema la detecta automáticamente
- Los datos se procesan y validan
- El dashboard se actualiza sin intervención manual

Esto asegura un sistema confiable, eficiente y útil para el análisis de la percepción de seguridad en la población.
