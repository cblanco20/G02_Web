document.addEventListener("DOMContentLoaded", function () {

    const contenedor = document.getElementById("listaIngredientes");
    const plantilla = document.getElementById("plantillaIngrediente");
    const botonAgregar = document.getElementById("agregarIngrediente");

    if (!contenedor || !plantilla || !botonAgregar) {
        return;
    }

    function actualizarUnidad(fila) {
        const select = fila.querySelector(".ingrediente-select");
        const etiqueta = fila.querySelector(".ingrediente-unidad");
        const opcion = select.options[select.selectedIndex];

        etiqueta.textContent = opcion && opcion.dataset.unidad ? opcion.dataset.unidad : "";
    }

    function reindexar() {
        contenedor.querySelectorAll(".fila-ingrediente").forEach(function (fila, indice) {
            fila.querySelector(".ingrediente-select").name = "Ingredientes[" + indice + "].IdIngrediente";
            fila.querySelector(".ingrediente-cantidad").name = "Ingredientes[" + indice + "].Cantidad";
        });
    }

    function enlazar(fila) {
        fila.querySelector(".ingrediente-select").addEventListener("change", function () {
            actualizarUnidad(fila);
        });

        fila.querySelector(".quitar-ingrediente").addEventListener("click", function () {
            fila.remove();

            if (contenedor.querySelectorAll(".fila-ingrediente").length === 0) {
                agregarFila();
            }

            reindexar();
        });

        actualizarUnidad(fila);
    }

    function agregarFila() {
        const fragmento = plantilla.content.cloneNode(true);
        const fila = fragmento.querySelector(".fila-ingrediente");

        contenedor.appendChild(fila);
        enlazar(fila);
        reindexar();
    }

    contenedor.querySelectorAll(".fila-ingrediente").forEach(enlazar);

    if (contenedor.querySelectorAll(".fila-ingrediente").length === 0) {
        agregarFila();
    }

    reindexar();

    botonAgregar.addEventListener("click", agregarFila);
});