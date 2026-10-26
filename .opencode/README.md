# Skill para edición de CHANGELOG

Skill para que un agente de IA cree o actualice el `CHANGELOG.md` de tu proyecto a partir del historial de git (tags y commits).

El agente necesita poder ejecutar `git` dentro de tu repositorio. Un chat sin acceso al repositorio no sirve, por eso mirar las recomendaciones de herramientas y las pautas del documento de consigna en Blackboard.

## Instalación

1. Copiá la carpeta `changelog/` dentro de una carpeta `.agents/skills/` en tu proyecto:

   ```
   mi-juego/
     .agents/skills/changelog/
       SKILL.md
       CHANGELOG-TEMPLATE.md
       CHANGELOG-EXAMPLE.md
       LICENSE.md
   ```

   Si usás algún agente en particular, puede que tengas que copiarla a un directorio específico. Por ejemplo, para Claude Code cópiala en `.claude/skills/changelog/`.

2. Hacé commit para que quede en el repositorio:

   ```bash
   git add .agents/skills/changelog
   git commit -m "Add changelog skill"
   ```

## Uso

1. La skill funciona revisando los tags para crear el changelog en base a las últimas versiones. Por lo tanto creá un tag para la nueva versión si aún no lo hiciste.

2. Pedile al agente:

   > Update the changelog by following the instructions in `.agents/skills/changelog/SKILL.md`. Read that file and the two files next to it first.

   En algunos agentes, como Claude Code, basta con escribir `/changelog`.

3. Respondé las preguntas del agente (problemas conocidos, IDs de tickets de los bugs corregidos).

4. Revisá el resultado que te muestra y, si está bien, hacé el commit del changelog actualizado:

   ```bash
   git add CHANGELOG.md
   git commit -m "Update changelog"
   ```

El agente solo modifica `CHANGELOG.md`. Nunca hace commit ni push.

Es importante que revises el resultado. Podés modificarlo manualmente para adaptarlo o mejorarlo, tranquilamente.

Autor: Sergio Baretto. Uso sujeto a la licencia en `changelog/LICENSE.md`.
