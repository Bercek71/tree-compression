document.addEventListener('DOMContentLoaded', function () {
    // Create the "back to top" button
    var button = document.createElement('button');
    button.id = 'back-to-top';
    button.innerHTML = '<i class="fas fa-arrow-up"></i>';
    document.body.appendChild(button);
  
    // Style the button with animations for smooth appearing and disappearing
    var style = document.createElement('style');
    style.innerHTML = `
      #back-to-top {
        position: fixed;
        bottom: 20px;
        right: 20px;
        background-color: #ff4081; /* Accent color */
        color: white;
        border: none;
        border-radius: 50%;
        padding: 15px;
        font-size: 18px;
        cursor: pointer;
        z-index: 1000;
        display: none;
        opacity: 0;
        transform: translateY(20px);
        transition: opacity 0.3s ease, transform 0.3s ease;
      }
  
      #back-to-top:hover {
        background-color: #e91e63;
        transform: scale(1.1); /* Button grows a little when hovered */
      }
  
      #back-to-top.show {
        display: block;
        opacity: 1;
        transform: translateY(0); /* Slide up when showing */
      }
    `;
    document.head.appendChild(style);
  
    // Show the button when scrolled down and animate its appearance
    window.onscroll = function () {
      if (document.body.scrollTop > 200 || document.documentElement.scrollTop > 200) {
        button.classList.add('show');
      } else {
        button.classList.remove('show');
      }
    };
  
    // Scroll to the top when the button is clicked
    button.onclick = function () {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    };
  });