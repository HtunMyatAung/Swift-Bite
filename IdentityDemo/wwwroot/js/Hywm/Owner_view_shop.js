$(document).ready(function () {
    $('.menu-item').each(function () {
        var card = $(this);

        card.on('mouseenter', function () {
            var itemQuantity = card.data('itemquantity');
            var discountRate = card.data('discount_rate');
            var discountPrice = card.data('discount_price');
            var finalPrice = card.data('final_price');

            var discountValue = discountRate > 0 ? `${discountRate}%` : `${discountPrice} MMK`;

            var detailsContent = `
                            <p>Total Quantity: ${itemQuantity} units</p>
                            <p>Final Price: ${finalPrice} MMK</p>
                            <p>Discount: ${discountValue}</p>
                        `;

            card.find('.get-details').html(detailsContent);
        });

        card.on('mouseleave', function () {
            card.find('.get-details').empty();
        });
    });
});