# Documentation Images

This directory contains images and diagrams used in the documentation.

## Adding Images

### Screenshots

When adding screenshots:
1. Use descriptive filenames (e.g., `chat-interface-spanish.png`)
2. Optimize images for web (compress before committing)
3. Recommended size: Max width 1200px
4. Format: PNG for screenshots, SVG for diagrams when possible

### Mermaid Diagram Exports

While Mermaid diagrams render directly in Markdown on GitHub, you can export them as images for offline documentation:

1. **Using Mermaid Live Editor**:
   - Visit https://mermaid.live/
   - Paste your Mermaid code
   - Export as PNG or SVG

2. **Using VS Code Extension**:
   - Install "Markdown Preview Mermaid Support"
   - Right-click diagram in preview
   - Save as image

### Naming Conventions

Use this pattern: `{topic}-{description}.{ext}`

Examples:
- `architecture-clean-layers.png`
- `chat-flow-sequence.svg`
- `deployment-azure-resources.png`
- `ui-screenshot-welcome-page.png`

## Referencing Images in Documentation

### Relative Path from Docs
```markdown
![Architecture Diagram](images/architecture-clean-layers.png)
```

### With Alt Text and Title
```markdown
![Clean Architecture Layers](images/architecture-clean-layers.png "AidCircle Clean Architecture")
```

## Image Optimization

Before committing images, optimize them:

**Online Tools**:
- https://tinypng.com/ (PNG compression)
- https://jakearchibald.github.io/svgomg/ (SVG optimization)

**Command Line**:
```powershell
# ImageMagick (install first: winget install ImageMagick.ImageMagick)
magick convert input.png -resize 1200x -quality 85 output.png
```

## Current Images

(Add list of images here as they are added)

- [ ] Clean architecture diagram
- [ ] System architecture diagram
- [ ] Chat flow sequence diagram
- [ ] Database schema diagram
- [ ] Welcome page screenshot
- [ ] Chat interface screenshot (multiple languages)
- [ ] Admin dashboard screenshot
