window.initializeDraggableWidgets = () => {
    interact('.widget-draggable').draggable({
        inertia: true,
        modifiers: [
            interact.modifiers.snap({
                targets: [interact.snappers.grid({ x: 20, y: 20 })],
                range: Infinity,
                relativePoints: [{ x: 0, y: 0 }]
            }),
            interact.modifiers.restrict({
                restriction: 'parent',
                endOnly: true
            })
        ],
        listeners: {
            move(event) {
                const target = event.target;
                const x = (parseFloat(target.getAttribute('data-x')) || 0) + event.dx;
                const y = (parseFloat(target.getAttribute('data-y')) || 0) + event.dy;

                target.style.transform = `translate(${x}px, ${y}px)`;
                target.setAttribute('data-x', x);
                target.setAttribute('data-y', y);
            },
            end(event) {
                const target = event.target;
                const id = target.getAttribute('data-id');
                const x = parseFloat(target.getAttribute('data-x')) || 0;
                const y = parseFloat(target.getAttribute('data-y')) || 0;

                // Llama a Blazor para actualizar el widget en C#
                DotNet.invokeMethodAsync('SAPAssistant', 'UpdateWidgetPosition', id, x, y);
            }
        }
    });
}
