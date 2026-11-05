document.addEventListener("DOMContentLoaded", function () {

    const buscador = document.getElementById("buscarUsuarios");
    const filas = document.querySelectorAll("#tablaUsuarios tbody tr");
    const sinResultados = document.getElementById("sinResultados");

    if (!buscador) {
        return;
    }

    buscador.addEventListener("input", function () {

        const texto = buscador.value.toLowerCase().trim();
        let encontrados = 0;

        filas.forEach(function (fila) {

            const contenido = fila.textContent.toLowerCase();

            if (contenido.includes(texto)) {
                fila.style.display = "";
                encontrados++;
            }
            else {
                fila.style.display = "none";
            }

        });

        if (sinResultados) {
            sinResultados.classList.toggle(
                "d-none",
                encontrados > 0 || filas.length === 0
            );
        }

    });

});