import type {
  MessageHandler,
  MessagePlacement,
  NotificationHandle,
} from 'element-plus'

import { ElMessage, ElNotification } from 'element-plus'

const _defaultMessageOptions = {
  duration: 4000,
  showClose: true,
  placement: 'top',
}

const _defaultNotificationOptions = {
  duration: 4500,
  showClose: true,
  position: 'top-right',
}

type NotifyType = MessageHandler

function showMessage(
  type: any,
  message: any,
  duration?: number,
  showClose?: boolean,
  placement?: MessagePlacement,
): NotifyType {
  return ElMessage({
    type,
    message,
    showClose: showClose || _defaultMessageOptions.showClose,
    duration: duration || _defaultMessageOptions.duration,
    placement: placement || (_defaultMessageOptions.placement as any),
  })
}

function showNotification(
  type: any,
  message: any,
  title: any,
  duration?: number,
  showClose?: boolean,
  position?: MessagePlacement,
): NotificationHandle {
  return ElNotification({
    type,
    title,
    message,
    showClose: showClose || _defaultNotificationOptions.showClose,
    duration: duration || _defaultNotificationOptions.duration,
    position: position || (_defaultNotificationOptions.position as any),
  })
}

class WindowNotify {
  error(
    content: any,
    duration?: number,
    showClose?: boolean,
    placement?: MessagePlacement,
  ): NotifyType {
    return showMessage('error', content, duration, showClose, placement)
  }

  info(
    content: any,
    duration?: number,
    showClose?: boolean,
    placement?: MessagePlacement,
  ): NotifyType {
    return showMessage('primary', content, duration, showClose, placement)
  }

  notificationError(
    message: any,
    title?: any,
    duration?: number,
    showClose?: boolean,
    position?: MessagePlacement,
  ): NotificationHandle {
    return showNotification(
      'error',
      message,
      title,
      duration,
      showClose,
      position,
    )
  }

  notificationInfo(
    message: any,
    title?: any,
    duration?: number,
    showClose?: boolean,
    position?: MessagePlacement,
  ): NotificationHandle {
    return showNotification(
      'info',
      message,
      title,
      duration,
      showClose,
      position,
    )
  }

  notificationSuccess(
    message: any,
    title?: any,
    duration?: number,
    showClose?: boolean,
    position?: MessagePlacement,
  ): NotificationHandle {
    return showNotification(
      'success',
      message,
      title,
      duration,
      showClose,
      position,
    )
  }

  notificationWarn(
    message: any,
    title?: any,
    duration?: number,
    showClose?: boolean,
    position?: MessagePlacement,
  ): NotificationHandle {
    return showNotification(
      'warn',
      message,
      title,
      duration,
      showClose,
      position,
    )
  }

  success(
    content: any,
    duration?: number,
    showClose?: boolean,
    placement?: MessagePlacement,
  ): NotifyType {
    return showMessage('success', content, duration, showClose, placement)
  }

  warn(
    content: any,
    duration?: number,
    showClose?: boolean,
    placement?: MessagePlacement,
  ): NotifyType {
    return showMessage('warn', content, duration, showClose, placement)
  }
}

export const notify = new WindowNotify()
