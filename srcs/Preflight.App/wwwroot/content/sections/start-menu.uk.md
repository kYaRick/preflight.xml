# Меню Пуск

## Що змінює цей розділ
Керує тим, як виглядає **меню Пуск** і **панель завдань** на першому завантаженні -
поле пошуку, закріплені значки, віджети, плитки (Windows 10) та закріплення (Windows 11).

## Варіанти пошуку на панелі завдань
Windows дає чотири варіанти відображення пошуку поруч із кнопкою Пуск.
Виберіть той, що пасує вашому збірному сценарію:

![Повне поле пошуку](content/images/taskbar-search-box.png)
*Повне поле - стандарт на свіжих Windows 11.*

![Значок з підписом](content/images/taskbar-search-label.png)
*Значок з підписом - компактно, але видно де шукати.*

![Лише значок](content/images/taskbar-search-icon.png)
*Лише значок - стандарт Windows 10.*

![Приховано](content/images/taskbar-search-hide.png)
*Приховано - нуль місця на панелі; пошук все одно доступний через `Win`.*

## Коли це корисно
- Щоб хром Пуск/панелі завдань був однаковий на багатьох пристроях.
- Щоб замінити стандартні закріплення Microsoft на власний `taskbar-layout.xml` / `start2.json`.
- Щоб сховати Віджети / Task View / Bing-результати без пост-інсталу.

## Примітки та ризики
- Плитки Пуск (XML Windows 10) ігноруються на Windows 11.
- Закріплення Пуск (`start2.json` Win 11) ігноруються Windows 10.
- "Видалити всі" очищає макет, але користувач може закріпити що завгодно після першого входу.

## Зовнішні джерела
- [Microsoft Learn: Схема LayoutModificationTemplate](https://learn.microsoft.com/windows/configuration/customize-and-export-start-layout)
- [Microsoft Learn: Налаштування макета Пуск у Windows 11](https://learn.microsoft.com/windows/configuration/customize-start-menu-layout-windows-11)
