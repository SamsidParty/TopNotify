import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import transformPlugin from 'vite-plugin-transform';
import { resolve, join } from 'path';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        react(),
        transformPlugin({
            tStart: '%{',
            tEnd: '}%',
            replace: {
                "TOPNOTIFY_VERSION": "2.6.0"
            },
            replaceFiles: [
                resolve(join(__dirname, '..\\..\\TopNotify\\dist\\Meta\\AppxManifest.xml')),
                resolve(join(__dirname, '..\\..\\TopNotify\\dist\\Meta\\manifest.xml')),
                resolve(join(__dirname, '..\\..\\TopNotify\\dist\\Meta\\WinGet\\SamsidParty.TopNotifyWG.yaml')),
                resolve(join(__dirname, '..\\..\\TopNotify\\dist\\Meta\\WinGet\\SamsidParty.TopNotifyWG.installer.yaml')),
                resolve(join(__dirname, '..\\..\\TopNotify\\dist\\Meta\\WinGet\\SamsidParty.TopNotifyWG.locale.en-US.yaml')),
            ]
        })
    ],
    build: {
        outDir: '..\\..\\TopNotify\\dist'
    }
})
