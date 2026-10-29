import './app.css';
import { createApp, h } from 'vue';
import { createInertiaApp } from '@inertiajs/vue3';

createInertiaApp({
  // Mapeia automaticamente todas as páginas .vue dentro da pasta Pages
  resolve: name => {
    const pages = import.meta.glob('./Pages/**/*.vue', { eager: true });
    let page = pages[`./Pages/${name}.vue`];
    if (!page) {
      const lower = `./Pages/${name}.vue`.toLowerCase();
      const matchedKey = Object.keys(pages).find(k => k.toLowerCase() === lower);
      if (matchedKey) {
        page = pages[matchedKey];
      }
    }
    if (!page) {
      console.error(`Página Inertia não encontrada: ./Pages/${name}.vue`);
      throw new Error(`Página Inertia não encontrada: ${name}`);
    }
    return page;
  },
  setup({ el, App, props, plugin }) {
    createApp({ render: () => h(App, props) })
      .use(plugin)
      .mount(el);
  },
});
