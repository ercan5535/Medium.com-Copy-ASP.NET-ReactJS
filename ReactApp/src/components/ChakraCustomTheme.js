import { extendTheme } from '@chakra-ui/react';

export default extendTheme({
    components: {
        Drawer: {
            sizes: {
                menu: {
                  dialog: { maxWidth: "23%" }
                }
              }
          }
    }
  });