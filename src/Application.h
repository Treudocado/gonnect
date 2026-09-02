#pragma once

#include <QApplication>
#include <QSocketNotifier>
#include <QQuickWindow>
#include <QObject>
#include <QTranslator>

#ifdef Q_OS_WINDOWS
#  include <QTimer>
#endif

#define RESTART_CODE 255

// The number of miliseconds a call shall be visible after it has ended
#define GONNECT_CALL_VISIBLE_AFTER_END 3500

class Application : public QApplication
{
    Q_OBJECT

public:
    explicit Application(int &argc, char **argv);
    ~Application();

    bool isFirstInstance() const;
    void sendArguments() const;

#ifdef Q_OS_WINDOWS
    void handleActivationArguments(const QStringList &arguments);
#endif

    void setRootWindow(QQuickWindow *win);
    QQuickWindow *rootWindow() const { return m_rootWindow; }

    // Unix signal handlers
#ifdef Q_OS_LINUX
    static void hupSignalHandler(int unused);
    static void termSignalHandler(int unused);
#endif

    bool isDebugRun() const { return m_isDebugRun; }

    static QString logFilePath();
    static QString logFileName();

public Q_SLOTS:

#ifdef Q_OS_LINUX
    void handleSigHup();
    void handleSigTerm();
#endif

    void initialize();
    void shutdown();

private:
    static void logQtMessages(QtMsgType type, const QMessageLogContext &context,
                              const QString &rawMsg);

    void initLogging();
    void initializeSIP();
    void installTranslations();

#ifdef Q_OS_WINDOWS
    void armPendingActivation();
    void dispatchPendingActivation();
#endif

#ifdef Q_OS_LINUX
    static int s_sighupFd[2];
    static int s_sigtermFd[2];

    QSocketNotifier *m_hupNotifier = nullptr;
    QSocketNotifier *m_termNotifier = nullptr;
#endif

    QQuickWindow *m_rootWindow = nullptr;

    QTranslator m_appTranslator;
    QTranslator m_baseTranslator;
    QTranslator m_declarativeTranslator;

#ifdef Q_OS_WINDOWS
    QStringList m_pendingActivationArguments;
    QTimer m_activationTimeout;
    QMetaObject::Connection m_activationRegistrationConnection;
#endif

    bool m_initialized = false;
    bool m_isDebugRun = false;
};
