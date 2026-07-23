document.addEventListener("DOMContentLoaded", () => {
    const validarCodigo = document.getElementById("validarCodigo");
    const barcodeCanvas = document.getElementById("barcodeCanvas");
    const barcodeImage = document.getElementById("barcodeImage");
    let ImagenCodigoDeBarras = document.getElementById("ImagenCodigoDeBarras");
    const btnDescargar = document.getElementById("btnDescargar");

    let ImagenProducto = document.getElementById('ImagenProducto');
    let previewImagenProducto = document.getElementById('previewImagenProducto');
    let iconoPreviewImagenProducto = document.getElementById('iconoPreviewImagenProducto');
    let inputImagenProducto = document.getElementById('inputImagenProducto');
    let imagenProductoBase64 = null;
        
    
    // 🎯 Función para descargar la imagen
    async function descargarImagen(nombreArchivo) {
        const link = document.createElement("a");
        link.href = barcodeCanvas.toDataURL("image/png");
        link.download = `CodigoDeBarras_${nombreArchivo}.png`;
        link.click();
    }

    function CargarImagenBase64(canvaImagen, imagenBase) {
        if (canvaImagen.id == 'previewImagenProducto' && (imagenBase == null || imagenBase == '')) {
            iconoPreviewImagenProducto.style.display = 'block';
            previewImagenProducto.style.display = 'none';
        } else if (canvaImagen.id == 'previewImagenProducto' && imagenBase != null)  {
            iconoPreviewImagenProducto.style.display = 'none';
            previewImagenProducto.style.display = 'block';
        }

        if (imagenBase && canvaImagen) {
            let existingBase64 = imagenBase; // Obtiene la cadena Base64 del input hidden
            if (existingBase64) {
                let imageUrl = `data:image/png;base64,${existingBase64}`;
                canvaImagen.style.display = 'block'; // Muestra la imagen
                canvaImagen.src = imageUrl;
            } else {
                canvaImagen.style.display = 'none'; // Asegúrate de que esté oculta si no hay imagen
                console.log("No hay imagen Base64 existente en el hidden input.");
            }
        } else {
            console.warn(`Elementos HTML 'ImagenProducto' o ' ${canvaImagen} ' no encontrados.`);
        }
    }

    if (inputImagenProducto && previewImagenProducto) {
        inputImagenProducto.addEventListener('change', function (e) {
            if (e.target.files && e.target.files[0]) {
                const file = e.target.files[0];
                const reader = new FileReader();

                reader.onload = function (event) {
                    if (event.target.result != null) {
                        // 1. Mostrar la imagen en la vista previa
                        previewImagenProducto.src = event.target.result;
                        previewImagenProducto.style.display = 'block';

                        imagenProductoBase64 = event.target.result.split(',')[1];
                        ImagenProducto.value = imagenProductoBase64;
                        console.log("Imagen convertida a Base64 y almacenada.");
                        iconoPreviewImagenProducto.style.display = 'none';
                    } else {
                        iconoPreviewImagenProducto.style.display = 'block';
                        previewImagenProducto.style.display = 'none';
                    }
                };
                reader.readAsDataURL(file); // Lee el archivo como una URL de datos (Base64)
            } else {
                previewImagenProducto.src = '#';
                previewImagenProducto.style.display = 'none';
                iconoPreviewImagenProducto.style.display = 'block';
                imagenProductoBase64 = null;
                console.log("No se seleccionó ninguna imagen. Variable Base64 limpiada.");
            }
        });
    } else {
        console.warn("Elementos 'ImagenProducto' o 'previewImagenProducto' no encontrados en el DOM.");
    }

    window.onload = function () {
        if (ImagenProducto != null) {
            CargarImagenBase64(previewImagenProducto, ImagenProducto.value); // Cargar imagen del producto
        }
        if (ImagenCodigoDeBarras != null) {
            CargarImagenBase64(barcodeImage, ImagenCodigoDeBarras.value); // Cargar imagen del código de barras

            // 📝 Activar descarga
            btnDescargar.style.display = "block";
            btnDescargar.onclick = () => descargarImagen(validarCodigo.value);
        }
    };


    btnDescargar.addEventListener("click", (event) => {
        event.preventDefault(); // ❌ Evita que el formulario se envíe
    });


});